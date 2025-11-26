using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Actuarius.Collections
{
    public class SystemDictionary<TKey, TData> : IMap<TKey, TData>
        where TKey : IEquatable<TKey>
    {
        private readonly Dictionary<TKey, TData> mDictionary = new Dictionary<TKey, TData>();

        public bool Add(TKey key, TData element)
        {
            try
            {
                if (!mDictionary.ContainsKey(key))
                {
                    mDictionary.Add(key, element);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool Remove(TKey key)
        {
            return mDictionary.Remove(key);
        }

        public bool TryGetValue(TKey key,[MaybeNullWhen(false)] out TData element)
        {
            return mDictionary.TryGetValue(key, out element);
        }
    }
}