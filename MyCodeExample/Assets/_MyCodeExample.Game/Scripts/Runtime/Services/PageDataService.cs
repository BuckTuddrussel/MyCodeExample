using System;
using System.IO;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using MyCodeExample.Collections.SystemManager;
using Debug = UnityEngine.Debug;

namespace MyCodeExample.Game.Services
{
    /// <summary>
    /// Service responsible for managing page data.
    /// </summary>
    public sealed class PageDataService : IService
    {
        public const int INVALID_CACHE = int.MinValue;
        private readonly SemaphoreSlim semaphore;

        private ArraySegment<RequestPageDataResult> _cachedData;
        private IDataServer _dataServerProvider;

        /// <summary>
        /// Gets or sets the request timeout duration.
        /// </summary>
        public TimeSpan RequestTimeout { get; set; } = new TimeSpan(0, 0, 5);

        /// <summary>
        /// Gets the number of available data.
        /// </summary>
        public int DataAvailableCount { get; private set; } = INVALID_CACHE;

        /// <summary>
        /// Gets a value indicating whether the data available count is valid.
        /// </summary>
        public bool IsDataAvailableCountValid => DataAvailableCount > 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageDataService"/> class with the specified data server provider.
        /// </summary>
        /// <param name="dataServerProvider">The data server provider.</param>
        public PageDataService(IDataServer dataServerProvider)
        {
            semaphore = new SemaphoreSlim(1, 1);

            ChangeDataProvider(dataServerProvider);
        }

        /// <summary>
        /// Changes the data provider to the specified provider.
        /// </summary>
        /// <param name="dataServerProvider">The new data server provider.</param>
        public void ChangeDataProvider(IDataServer dataServerProvider)
        {
            Debug.Assert(dataServerProvider != null, $"{nameof(IDataServer)} cannot be null");

            semaphore.Wait();
            _dataServerProvider = dataServerProvider;
            semaphore.Release();

            InvalidateCache();
        }

        /// <summary>
        /// Invalidates the cache by resetting the data available count and cached data.
        /// </summary>
        public void InvalidateCache()
        {
            semaphore.Wait();

            DataAvailableCount = INVALID_CACHE;
            _cachedData = ArraySegment<RequestPageDataResult>.Empty;

            semaphore.Release();
        }

        /// <summary>
        /// Requests the data available count asynchronously.
        /// </summary>
        /// <param name="cts">The cancellation token source.</param>
        /// <returns>The task representing the asynchronous operation.</returns>
        public async Task<int> RequestDataAvailableCount(CancellationTokenSource cts)
        {
            var isSuccess = false;
            Exception taskFailedException = null;

            try
            {
                await semaphore.WaitAsync(cts.Token);

                var dataAvailableTask = _dataServerProvider.DataAvailable(cts.Token);
                var dataAvailableResult = await dataAvailableTask.TimeoutAfter(cts, RequestTimeout);

                if (dataAvailableTask.IsCompletedSuccessfully && dataAvailableResult > 0)
                {
                    DataAvailableCount = dataAvailableResult;
                    _cachedData = GenerateEmptyCacheEntries(dataAvailableResult);
                    isSuccess = true;
                }
                else
                {
                    taskFailedException = new InvalidDataException();
                }
            }
            catch (OperationCanceledException e)
            {
                taskFailedException = e;
            }
            catch (Exception e)
            {
                taskFailedException = e;
            }
            finally
            {
                semaphore.Release();
            }

            if (!isSuccess)
            {
                InvalidateCache();
                throw taskFailedException;
            }

            return DataAvailableCount;
        }

        /// <summary>
        /// Tries to request page data asynchronously.
        /// </summary>
        /// <param name="startIndex">The start index of the data.</param>
        /// <param name="count">The count of data entries.</param>
        /// <param name="cts">The cancellation token source.</param>
        /// <returns>The task representing the asynchronous operation with the requested page data.</returns>
        public async Task<IList<RequestPageDataResult>> TryRequestPageData(int startIndex, int count,
            CancellationTokenSource cts)
        {
            DevelopmentDataAccessValidation(startIndex, count);

            var isSuccess = false;
            var requestResult = Array.Empty<RequestPageDataResult>();
            Exception taskFailedException = null;

            try
            {
                await semaphore.WaitAsync(cts.Token);

                var taskRequestData = _dataServerProvider.RequestData(startIndex, count, cts.Token);
                await taskRequestData.TimeoutAfter(cts, RequestTimeout);

                var taskRequestResult = taskRequestData.Result;
                if (taskRequestData.IsCompletedSuccessfully && taskRequestResult.Count > 0)
                {
                    var cacheSlice = _cachedData.Slice(startIndex, count);
                    UpdateCacheEntries(ref cacheSlice, taskRequestResult, startIndex);

                    requestResult = cacheSlice.ToArray();
                    isSuccess = true;
                }
                else
                {
                    taskFailedException = new InvalidDataException();
                }
            }
            catch (Exception e)
            {
                taskFailedException = e;
            }
            finally
            {
                semaphore.Release();
            }

            if (!isSuccess)
            {
                throw taskFailedException;
            }

            return requestResult;
        }

        /// <summary>
        /// Tries to get page data synchronously.
        /// </summary>
        /// <param name="startIndex">The start index of the data.</param>
        /// <param name="count">The count of data entries.</param>
        /// <param name="cacheResult">The cached result if available. Otherwise empty array</param>
        /// <returns>True if the operation was successful and the data was retrieved from cache, otherwise false.</returns>
        public bool TryGetPageData(int startIndex, int count, out IList<RequestPageDataResult> cacheResult)
        {
            DevelopmentDataAccessValidation(startIndex, count);

            var isSuccess = false;
            cacheResult = Array.Empty<RequestPageDataResult>();

            try
            {
                semaphore.Wait();

                var cacheSlice = _cachedData.Slice(startIndex, count);
                if (IsCachedDataValid(ref cacheSlice))
                {
                    cacheResult = cacheSlice.ToArray();
                    isSuccess = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                semaphore.Release();
            }

            return isSuccess;
        }

        private static ArraySegment<RequestPageDataResult> GenerateEmptyCacheEntries(int entriesCount)
        {
            var arraySegment = new ArraySegment<RequestPageDataResult>(new RequestPageDataResult[entriesCount]);
            for (var i = 0; i < entriesCount; i++)
            {
                arraySegment[i] = new RequestPageDataResult();
            }

            return arraySegment;
        }

        private static bool IsCachedDataValid(ref ArraySegment<RequestPageDataResult> requestPageDataResults)
        {
            var isInvalid = false;
            for (var i = 0; i < requestPageDataResults.Count; i++)
            {
                if (requestPageDataResults[i].IsValid) continue;

                isInvalid = true;
                break;
            }

            return !isInvalid;
        }

        private static void UpdateCacheEntries(ref ArraySegment<RequestPageDataResult> arraySegmentToUpdate,
            IList<DataItem> dataItems, int startIndex)
        {
            var requestDataIndex = startIndex;

            for (var cacheDataIndex = 0; cacheDataIndex < dataItems.Count; cacheDataIndex++, requestDataIndex++)
            {
                var entry = arraySegmentToUpdate[cacheDataIndex];
                entry.IsValid = true;
                entry.Index = requestDataIndex;
                entry.DataItem = dataItems[cacheDataIndex];
                arraySegmentToUpdate[cacheDataIndex] = entry;
            }
        }

        [Conditional("UNITY_ASSERTIONS"), Conditional("DEVELOPMENT_BUILD")]
        private void DevelopmentDataAccessValidation(int startIndex, int count)
        {
            if (DataAvailableCount <= 0)
            {
                throw new DataException(
                    $"Data count is unavailable - Did you call it without {nameof(RequestDataAvailableCount)}?");
            }

            Debug.Assert(startIndex >= 0, "startIndex should be non-negative.");
            Debug.Assert(startIndex <= DataAvailableCount,
                "startIndex should be less than or equal to DataAvailableCount.");
            Debug.Assert(count >= 0, "count should be non-negative.");
            Debug.Assert(startIndex + count <= DataAvailableCount,
                "The sum of startIndex and count should be within DataAvailableCount.");
        }
    }

    public struct RequestPageDataResult
    {
        public bool IsValid;
        public int Index;
        public DataItem DataItem;
        public int DisplayIndex => Index + 1;
    }
}