using System.Threading;

namespace MyCodeExample.Threading
{
    public sealed class SafeCancellationTokenSource : CancellationTokenSource
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            IsDisposed = true;
        }
    }
}