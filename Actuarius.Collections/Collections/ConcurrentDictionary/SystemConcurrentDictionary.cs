using System.Diagnostics.CodeAnalysis;

namespace Actuarius.Collections
{
    public class SystemConcurrentDictionary<TKey, TData> : IConcurrentMap<TKey, TData>
    {
        private readonly System.Collections.Concurrent.ConcurrentDictionary<TKey, TData> _dictionary = new ();
        
        public bool Add(TKey key, TData element)
        {
            return _dictionary.TryAdd(key, element);
        }

        public bool Remove(TKey key)
        {
            return _dictionary.TryRemove(key, out var _);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TData element)
        {
            return _dictionary.TryGetValue(key, out element);
        }

        public bool AddOrReplace(TKey key, TData newElement, [MaybeNullWhen(false)] out TData oldElement)
        {
            TData tmp = default!;
            _dictionary.AddOrUpdate(key, _ => newElement, (_, old) =>
            {
                tmp = old;
                return newElement;
            });
            oldElement = tmp;
            return true;
        }

        public bool AddOrGet(TKey key, TData newElement, [MaybeNullWhen(false)] out TData resultElement)
        {
            resultElement = _dictionary.GetOrAdd(key, newElement);
            return true;
        }
    }
}