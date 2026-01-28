using System.Diagnostics.CodeAnalysis;

namespace Actuarius.Collections
{
    public class SystemConcurrentQueue<TData> : IConcurrentQueue<TData>
    {
        private readonly System.Collections.Concurrent.ConcurrentQueue<TData> _queue = new();
        
        public bool Put(TData value)
        {
            _queue.Enqueue(value);
            return true;
        }

        public bool TryPop([MaybeNullWhen(false)] out TData value)
        {
            return _queue.TryDequeue(out value);
        }

        public int Count => _queue.Count;
    }
}