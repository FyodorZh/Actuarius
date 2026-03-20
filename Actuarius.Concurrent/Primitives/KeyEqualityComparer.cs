using System;
using System.Collections.Generic;

namespace Actuarius.Concurrent
{
    public class KeyEqualityComparer<T, TKey> : IEqualityComparer<T>
        where TKey : notnull
    {
        protected readonly Func<T, TKey> _keyExtractor;

        public KeyEqualityComparer(Func<T, TKey> keyExtractor)
        {
            _keyExtractor = keyExtractor;
        }

        public virtual bool Equals(T? x, T? y)
        {
            if (x == null && y == null)
                return true;
            if (x != null && y != null)
            {
                var key1 = _keyExtractor(x);
                var key2 = _keyExtractor(y);
                return key1.Equals(key2);
            }

            return false;
        }

        public int GetHashCode(T obj)
        {
            return _keyExtractor(obj).GetHashCode();
        }
    }
    
    public class StrictKeyEqualityComparer<T, TKey> : KeyEqualityComparer<T, TKey>
        where TKey : IEquatable<TKey>
    {
        public StrictKeyEqualityComparer(Func<T, TKey> keyExtractor) 
            : base(keyExtractor)
        {
        }

        public override bool Equals(T? x, T? y)
        {
            if (x == null && y == null)
                return true;
            if (x != null && y != null)
            {
                var key1 = _keyExtractor(x);
                var key2 = _keyExtractor(y);
                return key1.Equals(key2);
            }

            return false;
        }
    }
}