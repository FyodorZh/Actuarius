using System;
using System.Net.Http.Headers;
using System.Threading;

namespace Actuarius.Concurrent
{
    /// <summary>
    /// Represents a thread-safe container for a value of a specified type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value to be stored in the AtomicBox. It must be a value type (struct).
    /// </typeparam>
    public class AtomicBox<T>
        where T : struct
    {
        /// <summary>
        /// Represents an internal field used to store the value of type <typeparamref name="T"/>.
        /// Access to this field is synchronized using a <see cref="ReaderWriterLockSlim"/> to ensure thread safety.
        /// </summary>
        private T _value;

        /// <summary>
        /// A synchronization mechanism used to manage concurrent read and write access
        /// to the underlying value in a thread-safe manner.
        /// </summary>
        private readonly ReaderWriterLockSlim _lock = new ();

        /// <summary>
        /// Gets or sets the value stored in the AtomicBox.
        /// This property ensures thread-safe read and write operations
        /// using a ReaderWriterLockSlim to synchronize access.
        /// </summary>
        public T Value
        {
            get
            {
                _lock.EnterReadLock();
                var res = _value;
                _lock.ExitReadLock();
                return res;
            }
            set
            {
                _lock.EnterWriteLock();
                _value = value;
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Updates the current value stored in the AtomicBox using the provided update function.
        /// This method ensures thread-safe modification of the value.
        /// </summary>
        /// <param name="update">
        /// A function that takes the current value of type <typeparamref name="T"/> as input
        /// and returns the updated value of type <typeparamref name="T"/>.
        /// The update function is applied within a write lock to ensure thread safety.
        /// </param>
        public void Update(Func<T, T> update)
        {
            bool locked = false;
            try
            {
                _lock.EnterWriteLock();
                locked = true;
                _value = update(_value);
            }
            finally
            {
                if (locked)
                {
                    _lock.ExitWriteLock();
                }
            }
        }
    }
}