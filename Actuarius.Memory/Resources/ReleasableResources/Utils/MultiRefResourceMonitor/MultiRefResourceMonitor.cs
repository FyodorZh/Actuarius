using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Actuarius.Memory.Internal;

namespace Actuarius.Memory
{
    /// <summary>
    /// Collects diagnostics for a single <see cref="MultiRefResource"/>.
    /// </summary>
    /// <remarks>
    /// The monitor observes the resource through its <see cref="IRefCounterView"/>, keeps a capped history of
    /// reference-count events and raises <see cref="Error"/> when misuse is detected. It must be created explicitly
    /// through <see cref="MultiRefResource.ConstructMonitor"/>; until then the resource is not diagnosed.
    /// </remarks>
    public sealed class MultiRefResourceMonitor : IMultiRefResourceMonitor
    {
        private const int MaxHistory = 100;

        private readonly IRefCounterView _owner;
        private readonly string _ownerName;
        private readonly Queue<ResourceUsageRecord>? _history;
        private volatile bool _detached;
        private int _lastCount;

        /// <summary>
        /// Gets a value indicating whether recorded events include a captured <see cref="StackTrace"/>.
        /// </summary>
        public bool TraceEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether the monitor records reference-count events at all.
        /// </summary>
        public bool CollectHistory { get; }

        /// <summary>
        /// Gets a value indicating whether the monitor's finalizer reports a <see cref="ResourceUsageErrorType.Leak"/>
        /// when the resource is collected while it still has owners.
        /// </summary>
        public bool TrackLeaks { get; }

        /// <summary>
        /// Raised when misuse is detected. The first argument identifies the owning resource.
        /// Handlers run synchronously on the reporting thread, except for
        /// <see cref="ResourceUsageErrorType.Leak"/>, which is raised on a thread-pool thread.
        /// </summary>
        public event Action<string, ResourceUsageErrorType>? Error;

        /// <summary>
        /// Initializes a new monitor bound to the supplied reference counter.
        /// </summary>
        /// <param name="counter">The counter to observe.</param>
        /// <param name="ownerName">A human-readable description of the owning resource, reported with each error.</param>
        /// <param name="collectHistory">Whether to record reference-count events at all. Defaults to <c>true</c>.</param>
        /// <param name="trackLeaks">Whether the finalizer reports leaks. Defaults to <c>true</c>.</param>
        /// <param name="traceEnabled">Whether recorded events include a captured <see cref="StackTrace"/>. Defaults to <c>true</c>.</param>
        public MultiRefResourceMonitor(
            IRefCounterView counter,
            string ownerName,
            bool collectHistory = true,
            bool trackLeaks = true,
            bool traceEnabled = true)
        {
            _owner = counter ?? throw new ArgumentNullException(nameof(counter));
            _ownerName = ownerName ?? throw new ArgumentNullException(nameof(ownerName));
            CollectHistory = collectHistory;
            TrackLeaks = trackLeaks;
            TraceEnabled = traceEnabled;
            _lastCount = counter.RefCount;
            _owner.Error += OnError;
            _owner.Updated += OnCountUpdated;

            if (collectHistory)
            {
                _history = new Queue<ResourceUsageRecord>();
            }

            if (!trackLeaks)
            {
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>Reports a leak when the monitor is collected while the resource is still alive.</summary>
        ~MultiRefResourceMonitor()
        {
            if (TrackLeaks)
            {
                ReportLeak();
            }
        }

        /// <summary>Gets the observed reference counter.</summary>
        public IRefCounterView Counter => _owner;

        /// <summary>Gets a value indicating whether <see cref="Detach"/> has been called.</summary>
        internal bool IsDetached => _detached;

        /// <summary>
        /// Returns a snapshot of the recent history, oldest entry first.
        /// </summary>
        public IReadOnlyList<ResourceUsageRecord> ExportHistory()
        {
            var history = _history;
            if (history == null)
            {
                return Array.Empty<ResourceUsageRecord>();
            }

            lock (history)
            {
                return new List<ResourceUsageRecord>(history);
            }
        }

        /// <summary>
        /// Stops observing the reference counter and suppresses <see cref="ReportLeak"/> and
        /// <see cref="AssertNoUsage"/>. After this call the monitor records nothing, and a monitor
        /// that was detached is neither reported by its finalizer nor returned by
        /// <see cref="MultiRefResource.ConstructMonitor"/> (which creates a fresh one instead).
        /// </summary>
        public void Detach()
        {
            _detached = true;
            _owner.Error -= OnError;
            _owner.Updated -= OnCountUpdated;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Reports a leak if the resource still has owners. Called from the finalizer, so the report is
        /// raised on a thread-pool thread.
        /// </summary>
        private void ReportLeak()
        {
            if (_detached)
            {
                return;
            }

            int count = _owner.RefCount;
            if (count > 0)
            {
                Record("Leak", count, true);
                RaiseOffThread(ResourceUsageErrorType.Leak);
            }
        }

        /// <summary>
        /// Reports a failure if the resource still has owners.
        /// </summary>
        public void AssertNoUsage()
        {
            if (_detached)
            {
                return;
            }

            int count = _owner.RefCount;
            if (count > 0)
            {
                Record("AssertNoUsage", count, true);
                RaiseError(ResourceUsageErrorType.NoUsageAssertionFail);
            }
        }

        private void OnCountUpdated(int count)
        {
            if (_detached)
            {
                return;
            }

            int oldCount = Interlocked.Exchange(ref _lastCount, count);
            Record(TransitionName(oldCount, count), count, false);
        }

        private void OnError(RefCounterErrorEvent error, int count)
        {
            if (_detached)
            {
                return;
            }

            Interlocked.Exchange(ref _lastCount, count);

            ResourceUsageErrorType type;
            switch (error)
            {
                case RefCounterErrorEvent.FailedToIncrement:
                    type = ResourceUsageErrorType.AddRefOfReleasedObject;
                    break;
                case RefCounterErrorEvent.FailedToDecrement:
                    type = ResourceUsageErrorType.ReleaseOfReleasedObject;
                    break;
                case RefCounterErrorEvent.FailedToRevive:
                    type = ResourceUsageErrorType.WrongReviveUsage;
                    break;
                default:
                    type = ResourceUsageErrorType.Unknown;
                    break;
            }

            Record(type.ToString(), count, true);
            RaiseError(type);
        }

        private void Record(string action, int count, bool isError)
        {
            var history = _history;
            if (history == null)
            {
                return;
            }

            StackTrace? stack = TraceEnabled ? new StackTrace(2, true) : null;
            lock (history)
            {
                history.Enqueue(new ResourceUsageRecord(action, count, isError, stack));
                if (history.Count > MaxHistory)
                {
                    history.Dequeue();
                }
            }
        }

        private void RaiseError(ResourceUsageErrorType error)
        {
            try
            {
                Error?.Invoke(_ownerName, error);
            }
            catch
            {
                Debug.WriteLine("MultiRefResourceMonitor Error handler threw.");
            }
        }

        private void RaiseOffThread(ResourceUsageErrorType error)
        {
            var handler = Error;
            if (handler == null)
            {
                return;
            }

            string name = _ownerName;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    handler(name, error);
                }
                catch
                {
                    Debug.WriteLine("MultiRefResourceMonitor Error handler threw.");
                }
            });
        }

        /// <summary>
        /// Discards the captured history. Reports do not drain it, so call this to drop accumulated records.
        /// </summary>
        public void Clear()
        {
            var history = _history;
            if (history == null)
            {
                return;
            }

            lock (history)
            {
                history.Clear();
            }
        }

        private static string TransitionName(int oldCount, int count)
        {
            int delta = count - oldCount;
            if (delta == 1)
            {
                return oldCount == 0 ? "Revive" : "AddRef";
            }

            if (delta == -1)
            {
                return "Release";
            }

            return "Updated";
        }
    }
}
