using System.Diagnostics.CodeAnalysis;

namespace Actuarius.Collections
{
    public class SystemConcurrentUnorderedCollection<TData> : IConcurrentUnorderedCollection<TData>
    {
        private readonly System.Collections.Concurrent.ConcurrentBag<TData> _bag = new();
        
        public bool Put(TData value)
        {
            _bag.Add(value);
            return true;
        }

        public bool TryPop([MaybeNullWhen(false)] out TData value)
        {
            return _bag.TryTake(out value);
        }

        public int Count => _bag.Count;
    }
}