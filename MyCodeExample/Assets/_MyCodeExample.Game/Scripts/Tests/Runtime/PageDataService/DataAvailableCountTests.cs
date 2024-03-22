using System;
using System.Threading;
using System.Threading.Tasks;
using MyCodeExample.Client;
using NUnit.Framework;

namespace MyCodeExample.Game.Tests.Runtime.PageDataService
{
    /// <summary>
    /// Some example tests
    /// </summary>
    [TestFixture]
    internal sealed class DataAvailableCountTests
    {
        private Services.PageDataService _pageDataService;
        
        [SetUp]
        public void Setup()
        {
            _pageDataService = new Services.PageDataService(new DataServerMock());
        }

        [Test]
        public async Task RequestDataAvailableCount_IsDataAvailableCountValid()
        {
            var cts = new CancellationTokenSource();
            await _pageDataService.RequestDataAvailableCount(cts);
            
            Assert.True(_pageDataService.IsDataAvailableCountValid);
        }
        
        [Test]
        public async Task RequestDataAvailableCount_Greater_Than_Zero()
        {
            var cts = new CancellationTokenSource();
            var result = await _pageDataService.RequestDataAvailableCount(cts);
            
            Assert.True(result > 0);
        }
        
        [Test]
        public async Task RequestDataAvailableCount_Abort()
        {
            var cts = new CancellationTokenSource();
            var task = _pageDataService.RequestDataAvailableCount(cts);
            cts.Cancel();
            
            await task;
            
            if (task.IsCanceled && task.IsCompleted)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }
        
        [Test]
        public async Task RequestDataAvailableCount_Abort_Did_InvalidateCache()
        {
            var cts = new CancellationTokenSource();
            var task = _pageDataService.RequestDataAvailableCount(cts);
            cts.Cancel();
            
            var result = await task;
            if (task.IsCanceled && task.IsCompleted && result == Services.PageDataService.INVALID_CACHE)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }
        
        [Test]
        public async Task RequestDataAvailableCount_Timeout()
        {
            var isTimeoutException = false;
            var cts = new CancellationTokenSource();
            _pageDataService.RequestTimeout = new TimeSpan(1);
            
            try
            {
                await _pageDataService.RequestDataAvailableCount(cts);;
            }
            catch (TimeoutException)
            {
                isTimeoutException = true;
            }
            catch
            {
                // Ignored
            }
            
            Assert.IsTrue(isTimeoutException);
        }
        
        [TearDown]
        public void TearDown()
        {
            _pageDataService = null;
        }
    }
}