using MyCodeExample.Threading;

namespace MyCodeExample
{
    public static class SafeCancellationTokenSourceExtension
    {
        public static void TryCancelAndDispose(this SafeCancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource == null) return;
            if (cancellationTokenSource.IsDisposed) return;

            if (!cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
            }

            cancellationTokenSource.Dispose();
        }
    }
}