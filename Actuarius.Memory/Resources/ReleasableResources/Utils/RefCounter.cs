using System;

namespace Actuarius.Memory.Internal
{
    /// <summary>
    /// Identifies why an operation on a <see cref="RefCounter"/> failed.
    /// Delivered through the <see cref="RefCounter.Error"/> event.
    /// </summary>
    public enum RefCounterErrorEvent
    {
        /// <summary>
        /// An <see cref="RefCounter.AddRef"/> call was rejected because the count was less than or equal to zero.
        /// </summary>
        FailedToIncrement,

        /// <summary>
        /// A <see cref="RefCounter.Release"/> call decremented the count below zero.
        /// </summary>
        FailedToDecrement,

        /// <summary>
        /// A <see cref="RefCounter.Revive"/> call was rejected because the counter is not revivable
        /// or the count was not exactly zero.
        /// </summary>
        FailedToRevive
    }

    /// <summary>
    /// Read-only view of a <see cref="RefCounter"/>: the current count plus notifications of updates and failures.
    /// </summary>
    public interface IRefCounterView
    {
        /// <summary>
        /// Gets the current reference count. The counter is valid while this value is greater than zero.
        /// </summary>
        int RefCount { get; }

        /// <summary>
        /// Raised after a successful operation with the resulting count.
        /// </summary>
        /// <remarks>
        /// Notification is best-effort: handlers run synchronously on the calling thread and are not serialized.
        /// </remarks>
        event Action<int>? Updated;

        /// <summary>
        /// Raised after a failed operation with the failure kind and the count observed at the time of failure.
        /// </summary>
        /// <remarks>
        /// Notification is best-effort: handlers run synchronously on the calling thread and are not serialized.
        /// </remarks>
        event Action<RefCounterErrorEvent, int>? Error;
    }

    /// <summary>
    /// Lock-free reference counter backed by <see cref="System.Threading.Interlocked"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A non-revivable counter starts at 1 (the creator holds the initial reference).
    /// A revivable counter starts at 0 and must be brought up with <see cref="Revive"/> before <see cref="AddRef"/> can succeed.
    /// </para>
    /// <para>
    /// <see cref="AddRef"/> is only valid for a caller that already holds a reference; it must not be used to acquire a released
    /// counter (use <see cref="Revive"/> for revivable counters). <see cref="Release"/> does not clamp at zero, so repeated releases
    /// drive the count negative and each such release is reported through the <see cref="Error"/> event. Integer overflow is not guarded.
    /// </para>
    /// </remarks>
    public class RefCounter : IRefCounterView
    {
        private readonly bool _revivable;
        private volatile int _refCount;

        /// <inheritdoc />
        public event Action<int>? Updated;

        /// <inheritdoc />
        public event Action<RefCounterErrorEvent, int>? Error;

        /// <inheritdoc />
        public int RefCount => _refCount;

        /// <summary>
        /// Gets a value indicating whether the reference count is greater than zero.
        /// </summary>
        public bool IsValid => _refCount > 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefCounter"/> class.
        /// </summary>
        /// <param name="revivable">
        /// When <c>true</c>, the count starts at 0 and <see cref="Revive"/> may reset it from 0 to 1.
        /// When <c>false</c>, the count starts at 1 and <see cref="Revive"/> always fails.
        /// </param>
        public RefCounter(bool revivable)
        {
            _revivable = revivable;
            _refCount = revivable ? 0 : 1;
        }

        /// <summary>
        /// Atomically increments the reference count.
        /// </summary>
        /// <returns>
        /// The new count on success, or 0 if the counter was already released (count less than or equal to zero).
        /// On failure the count is left unchanged and the <see cref="Error"/> event is raised with
        /// <see cref="RefCounterErrorEvent.FailedToIncrement"/>.
        /// </returns>
        public int AddRef()
        {
            while (true)
            {
                int oldCount = _refCount;
                if (oldCount <= 0)
                {
                    Error?.Invoke(RefCounterErrorEvent.FailedToIncrement, oldCount); 
                    return 0;
                }

                if (System.Threading.Interlocked.CompareExchange(ref _refCount, oldCount + 1, oldCount) == oldCount)
                {
                    Updated?.Invoke(oldCount + 1);
                    return oldCount + 1;
                }
            }
        }

        /// <summary>
        /// Atomically decrements the reference count.
        /// </summary>
        /// <returns>
        /// The new count on success, including 0 when the last reference was released.
        /// Returns -1 if the count underflowed below zero, also raising the <see cref="Error"/> event with
        /// <see cref="RefCounterErrorEvent.FailedToDecrement"/>.
        /// </returns>
        public int Release()
        {
            var count = System.Threading.Interlocked.Decrement(ref _refCount);
            if (count < 0)
            {
                Error?.Invoke(RefCounterErrorEvent.FailedToDecrement, count);
                return -1;
            }

            Updated?.Invoke(count);
            return count;
        }

        /// <summary>
        /// Atomically resets the count from 0 to 1.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the count was exactly 0 and was set to 1; otherwise <c>false</c>.
        /// On failure the <see cref="Error"/> event is raised with <see cref="RefCounterErrorEvent.FailedToRevive"/>.
        /// </returns>
        public bool Revive()
        {
            if (!_revivable)
            {
                Error?.Invoke(RefCounterErrorEvent.FailedToRevive, _refCount);
                return false;
            }
            
            while (true)
            {
                int oldCount = _refCount;
                if (oldCount != 0)
                {
                    Error?.Invoke(RefCounterErrorEvent.FailedToRevive, oldCount);
                    return false;
                }

                if (System.Threading.Interlocked.CompareExchange(ref _refCount, oldCount + 1, oldCount) == oldCount)
                {
                    Updated?.Invoke(oldCount + 1);
                    return true;
                }
            }
        }

        /// <summary>
        /// Returns the string <c>"RefCount:"</c> followed by the current count.
        /// </summary>
        public override string ToString()
        {
            return "RefCount:" + _refCount;
        }
    }
}
