using System.Threading;
using Actuarius.Memory.Internal;

namespace Actuarius.Memory
{
    public abstract class MultiRefResource : IMultiRefResource_Debug
    {
        private readonly RefCounter _refCounter;
        private MultiRefResourceMonitor? _monitor;

        public IRefCounterView RefCount => _refCounter;

        protected MultiRefResource(bool revivable)
        {
            _refCounter = new RefCounter(revivable);
        }

        protected abstract void OnReleased();

        public bool IsAlive => _refCounter.IsValid;

        public void AddRef()
        {
            _refCounter.AddRef();
        }

        public void Release()
        {
            if (_refCounter.Release() == 0)
            {
                OnReleased();
            }
        }

        protected bool Revive()
        {
            return _refCounter.Revive();
        }

        IMultiRefResourceMonitor IMultiRefResource.ConstructMonitor(
            bool collectHistor,
            bool trackLeaks,
            bool traceEnabled)
        {
            return ConstructMonitor(collectHistor, trackLeaks, traceEnabled);
        }

        /// <summary>
        /// Creates (or returns the existing) diagnostics monitor bound to this resource.
        /// Pass the returned monitor to the owner that needs to observe misuse.
        /// </summary>
        /// <param name="collectHistory">Whether the monitor records reference-count events. Defaults to <c>true</c>.</param>
        /// <param name="trackLeaks">Whether the monitor's finalizer reports leaks. Defaults to <c>true</c>.</param>
        /// <param name="traceEnabled">Whether recorded events include a captured stack trace. Defaults to <c>true</c>.</param>
        /// <remarks>
        /// The options apply only when a monitor is actually created; if a live monitor already exists it is
        /// returned as-is. A monitor that has been detached is replaced with a fresh one, so calling
        /// <see cref="MultiRefResourceMonitor.Detach"/> followed by this method re-enables diagnostics.
        /// </remarks>
        public MultiRefResourceMonitor ConstructMonitor(
            bool collectHistory = true,
            bool trackLeaks = true,
            bool traceEnabled = true)
        {
            while (true)
            {
                var current = Volatile.Read(ref _monitor);

                if (current == null || current.IsDetached)
                {
                    var created = new MultiRefResourceMonitor(
                        _refCounter, GetType().ToString(), collectHistory, trackLeaks, traceEnabled);
                    var existing = Interlocked.CompareExchange(ref _monitor, created, current);
                    if (ReferenceEquals(existing, current))
                    {
                        return created;
                    }

                    created.Detach();
                    continue;
                }

                if (!current.IsDetached)
                {
                    return current;
                }
            }
        }

        /// <summary>Gets the live monitor created by <see cref="ConstructMonitor"/>, or null if none exists or it was detached.</summary>
        protected MultiRefResourceMonitor? Monitor
        {
            get
            {
                var monitor = Volatile.Read(ref _monitor);
                return monitor != null && !monitor.IsDetached ? monitor : null;
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name}: {_refCounter}";
        }
    }
}
