using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Actuarius.Memory
{
    /// <summary>
    /// Debug wrapper around an <see cref="IConcurrentPool{TResource, TParam0}"/> that hands out
    /// <see cref="MultiRefByteArrayGuard"/> instances and collects their diagnostics, so tests can verify that
    /// acquired resources are used and released correctly.
    /// </summary>
    public sealed class ByteArrayConcurrentPoolGuard : IConcurrentPool<IMultiRefByteArray, int>
    {
        private readonly IConcurrentPool<IMultiRefByteArray, int> _pool;
        private readonly bool _collectHistory;
        private readonly bool _traceEnabled;
        private readonly ConcurrentDictionary<MultiRefResourceMonitor, byte> _monitors = new ConcurrentDictionary<MultiRefResourceMonitor, byte>();
        private int _errors;

        public ByteArrayConcurrentPoolGuard(
            IConcurrentPool<IMultiRefByteArray, int> pool,
            bool collectHistory = true,
            bool traceEnabled = true)
        {
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _collectHistory = collectHistory;
            _traceEnabled = traceEnabled;
        }

        /// <summary>Gets the number of misuse reports raised by guards created by this pool.</summary>
        public int ErrorCount => Volatile.Read(ref _errors);

        /// <summary>Raised whenever a guard created by this pool reports misuse.</summary>
        public event Action<string, ResourceUsageErrorType>? Error;

        public IMultiRefByteArray Acquire(int param0)
        {
            return Guard(_pool.Acquire(param0));
        }

        public void Release(IMultiRefByteArray? resource)
        {
            if (resource != null && resource is not MultiRefByteArrayGuard)
            {
                throw new InvalidOperationException($"Wrong resource type '{resource.GetType()}', expected {nameof(MultiRefByteArrayGuard)}");
            }
            // do nothing. drop resource instance
        }

        // The returned pool sink intentionally does nothing (mirrors ByteArrayPool.Release): resources are
        // released via IMultiRefByteArray.Release(), not via pool.Release()/sink.Release(). Do not make it forward.
        public (IMultiRefByteArray resource, IPoolSink<IMultiRefByteArray> poolSink) AcquireEx(int param0)
        {
            var pair = _pool.AcquireEx(param0);
            return (Guard(pair.resource), this);
        }

        /// <summary>
        /// Reports <see cref="ResourceUsageErrorType.NoUsageAssertionFail"/> for every guard acquired from this pool
        /// that still has an owner (i.e. was never released). Call at the end of a test to detect leaks.
        /// </summary>
        public void AssertNoOutstanding()
        {
            foreach (var monitor in _monitors.Keys)
            {
                monitor.AssertNoUsage();
            }
        }

        private MultiRefByteArrayGuard Guard(IMultiRefByteArray resource)
        {
            var guard = new MultiRefByteArrayGuard(resource);
            var monitor = guard.ConstructMonitor(_collectHistory, trackLeaks: false, _traceEnabled);
            monitor.Error += OnMonitorError;
            _monitors.TryAdd(monitor, 0);

            // Untrack the guard once it is fully released: a released guard can never fail AssertNoUsage, and keeping
            // its monitor would grow this set without bound. The monitor stays attached (we do not Detach it), so any
            // post-release misuse - a second Release or an AddRef after release - is still reported through Error.
            // A non-revivable counter raises Updated exactly once, on the 1 -> 0 transition.
            guard.RefCount.Updated += OnGuardUpdated;
            return guard;

            void OnGuardUpdated(int count)
            {
                if (count != 0)
                {
                    return;
                }

                guard.RefCount.Updated -= OnGuardUpdated;
                _monitors.TryRemove(monitor, out _);
            }
        }

        private void OnMonitorError(string owner, ResourceUsageErrorType type)
        {
            Interlocked.Increment(ref _errors);
            Error?.Invoke(owner, type);
        }
    }
}
