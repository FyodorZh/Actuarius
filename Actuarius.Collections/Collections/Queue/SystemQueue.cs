using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Actuarius.Collections
{
    public class SystemQueue<TData> : IQueue<TData>
    {
        private readonly Queue<TData> _queue = new Queue<TData>();
        
        public int Count => _queue.Count;

        public bool Put(TData value)
        {
            _queue.Enqueue(value);
            return true;
        }

        public bool TryPop([MaybeNullWhen(false)] out TData value)
        {
            if (_queue.Count > 0)
            {
                value = _queue.Dequeue();
                return true;
            }

            value = default;
            return false;
        }

        public void Clear()
        {
            _queue.Clear();
        }
    }
}