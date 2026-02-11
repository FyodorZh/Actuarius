using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Actuarius.Collections
{
    public class SynchronizedConcurrentDictionary<TKey, TData> : IConcurrentMap<TKey, TData>
        //where TKey : IEquatable<TKey>
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
                if (!_dictionary.ContainsKey(key))
                {
                    _dictionary.Add(key, element);
                    return true;
                }
                return false;
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

        public bool AddOrReplace(TKey key, TData newElement, [MaybeNullWhen(false)] out TData oldElement)
        {
            bool bLocked = false;
            try
            {
                _locker.EnterWriteLock();
                bLocked = true;
                if (_dictionary.TryGetValue(key, out oldElement))
                {
                    _dictionary[key] = newElement; // replace
                }
                else
                {
                    _dictionary.Add(key, newElement); // add
                    oldElement = newElement;
                }
                return true;
            }
            catch
            {
                oldElement = default;
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

        public bool ReplaceOrAdd(TKey key, TData newElement, [MaybeNullWhen(false)] out TData oldElement)
        {
            throw new System.NotImplementedException();
        }

        public bool AddOrGet(TKey key, TData newElement, [MaybeNullWhen(false)] out TData resultElement)
        {
            bool bLocked = false;
            try
            {
                _locker.EnterWriteLock();
                bLocked = true;
                if (!_dictionary.TryGetValue(key, out resultElement))
                {
                    _dictionary.Add(key, newElement); // add
                    resultElement = newElement;
                }
                return true;
            }
            catch
            {
                resultElement = default;
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
    }
}