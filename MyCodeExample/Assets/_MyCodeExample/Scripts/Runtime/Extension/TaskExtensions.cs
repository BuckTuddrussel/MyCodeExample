using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using MyCodeExample.Threading;

namespace MyCodeExample
{
    /// <summary>
    /// Inefficient Task extension class to prevent code repetition and simplifying stuff
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Observes the task to avoid the UnobservedTaskException event to be raised.
        /// </summary>
        public static void Forget(this Task task)
        {
            // note: this code is inspired by a tweet from Ben Adams: https://twitter.com/ben_a_adams/status/1045060828700037125
            // Only care about tasks that may fault (not completed) or are faulted,
            // so fast-path for SuccessfullyCompleted and Canceled tasks.
            if (!task.IsCompleted || task.IsFaulted)
            {
                // use "_" (Discard operation) to remove the warning IDE0058: Because this call is not awaited, execution of the current method continues before the call is completed
                // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/discards?WT.mc_id=DT-MVP-5003978#a-standalone-discard
                _ = ForgetAwaited(task);
            }

            return;

            // Allocate the async/await state machine only when needed for performance reasons.
            // More info about the state machine: https://blogs.msdn.microsoft.com/seteplia/2017/11/30/dissecting-the-async-methods-in-c/?WT.mc_id=DT-MVP-5003978
            static async Task ForgetAwaited(Task task)
            {
                try
                {
                    // No need to resume on the original SynchronizationContext, so use ConfigureAwait(false)
                    await task.ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// Wraps the specified task and monitor for any exceptions that may occur during its execution.
        /// If an exception occurs, it is logged using <see cref="Debug.LogException(Exception)"/>.
        /// </summary>
        /// <param name="task">The task to be monitored.</param>
        /// <returns>
        /// The original task. 1
        /// </returns>
        public static async Task<Task> Monitor(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return task;
        }
        
        /// <summary>
        /// Wraps the specified task and monitor for any exceptions that may occur during its execution.
        /// If an exception occurs, it is logged using <see cref="Debug.LogException(Exception)"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <param name="task">The task to be monitored.</param>
        /// <returns>
        /// The result produced by the task if it completes successfully; otherwise, the default value of <typeparamref name="T"/>.
        /// </returns>
        public static async Task<TResult> Monitor<TResult>(this Task<TResult> task)
        {
            try
            {
                return await task;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return default;
        }

        /// <summary>
        /// Wraps the specified task and ignores any <see cref="OperationCanceledException"/> that may occur during its execution.
        /// If any other exception occurs, it is logged using <see cref="Debug.LogException(Exception)"/>.
        /// </summary>
        /// <typeparam name="T">The type of the result produced by the task.</typeparam>
        /// <param name="task">The task to be monitored.</param>
        /// <returns>The result produced by the task if it completes successfully; otherwise, the default value of <typeparamref name="T"/>.</returns>
        public static async Task<T> MonitorIgnoreCancellation<T>(this Task<T> task)
        {
            try
            {
                return await task;
            }
            catch (OperationCanceledException)
            {
                // Ignored
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return default;
        }

        /// <summary>
        /// Wraps the specified task with a timeout constraint. If the task does not complete within the specified timeout duration,
        /// a TimeoutException is thrown. Additionally, if the timeout occurs, the cancellation token source associated with the task
        /// is canceled to signal cancellation to the underlying operation (Note: Token is not disposed).
        /// </summary>
        /// <param name="task">The task to be executed with a timeout constraint.</param>
        /// <param name="taskCTS">The cancellation token source associated with the task.</param>
        /// <param name="timeout">The duration after which the task is considered timed out.</param>
        /// <exception cref="TimeoutException">Thrown if the task does not complete within the specified timeout duration.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled due to a timeout or an external cancellation request.</exception>
        public static async Task TimeoutAfter(this Task task, CancellationTokenSource taskCTS, TimeSpan timeout)
        {
            var timeoutCTS = new SafeCancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCTS.Token));

            if (completedTask != task)
            {
                taskCTS.Cancel();
                timeoutCTS.Dispose();
                throw new TimeoutException("The operation has timed out.");
            }

            timeoutCTS.TryCancelAndDispose();

            taskCTS.Token.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Wraps the specified task with a timeout constraint. If the task does not complete within the specified timeout duration,
        /// a TimeoutException is thrown. Additionally, if the timeout occurs, the cancellation token source associated with the task
        /// is canceled to signal cancellation to the underlying operation (Note: Token is not disposed).
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <param name="task">The task to be executed with a timeout constraint.</param>
        /// <param name="taskCTS">The cancellation token source associated with the task.</param>
        /// <param name="timeout">The duration after which the task is considered timed out.</param>
        /// <returns>The result produced by the task if it completes within the specified timeout duration.</returns>
        /// <exception cref="TimeoutException">Thrown if the task does not complete within the specified timeout duration.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled due to a timeout or an external cancellation request.</exception>
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task,
            CancellationTokenSource taskCTS, TimeSpan timeout)
        {
            var timeoutCTS = new SafeCancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCTS.Token));

            if (completedTask != task)
            {
                taskCTS.Cancel();
                timeoutCTS.Dispose();
                throw new TimeoutException("The operation has timed out.");
            }

            timeoutCTS.TryCancelAndDispose();

            taskCTS.Token.ThrowIfCancellationRequested();
            return task.Result;
        }
    }
}