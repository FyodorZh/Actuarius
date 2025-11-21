using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Actuarius.Collections
{
    public class SynchronizedConcurrentDictionary<TKey, TData> : IConcurrentMap<TKey, TData>
        where TKey : IEquatable<TKey>
    {
        private readonly Dictionary<TKey, TData> _dictionary = new Dictionary<TKey, TData>();
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        public bool Add(TKey key, TData element)
        {
            bool bLocked = false;
            try
            {
                _locker.EnterWriteLock();
                bLocked = true;
                _dictionary.Add(key, element);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (bLocked)
                {
                    _locker.ExitWriteLock();
                }
            }
        }

        public bool Remove(TKey key)
        {
            bool bLocked = false;
            try
            {
                _locker.EnterWriteLock();
                bLocked = true;
                return _dictionary.Remove(key);
            }
            catch
            {
                return false;
            }
            finally
            {
                if (bLocked)
                {
                    _locker.ExitWriteLock();
                }
            }
        }

        public bool TryGetValue(TKey key,[MaybeNullWhen(false)] out TData element)
        {
            bool bLocked = false;
            try
            {
                _locker.EnterReadLock();
                bLocked = true;
                return _dictionary.TryGetValue(key, out element);
            }
            catch
            {
                element = default;
                return false;
            }
            finally
            {
                if (bLocked)
                {
                    _locker.ExitReadLock();
                }
            }
        }
    }
}