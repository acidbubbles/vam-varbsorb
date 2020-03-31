using System;
using System.Threading;
using System.Threading.Tasks;

namespace Varbsorb
{
    public class Throttler : IThrottler
    {
        public const int MaxConcurrentIO = 4;

        private readonly SemaphoreSlim _ioThrottler = new SemaphoreSlim(MaxConcurrentIO);
        private readonly SemaphoreSlim _cpuThrottler = new SemaphoreSlim(Environment.ProcessorCount);

        public async Task<IDisposable> ThrottleIO()
        {
            await _ioThrottler.WaitAsync().ConfigureAwait(false);
            return new ReleaseWrapper(_ioThrottler);
        }

        public async Task<IDisposable> ThrottleCPU()
        {
            await _cpuThrottler.WaitAsync().ConfigureAwait(false);
            return new ReleaseWrapper(_cpuThrottler);
        }

        private class ReleaseWrapper : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            private bool _isDisposed;

            public ReleaseWrapper(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                if (_isDisposed) return;
                _semaphore.Release();
                _isDisposed = true;
            }
        }
    }

    public interface IThrottler
    {
        Task<IDisposable> ThrottleIO();
        Task<IDisposable> ThrottleCPU();
    }
}
