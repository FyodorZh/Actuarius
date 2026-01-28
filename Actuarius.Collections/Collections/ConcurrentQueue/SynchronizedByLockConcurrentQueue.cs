using System.Diagnostics.CodeAnalysis;

namespace Actuarius.Collections
{
    public class SynchronizedByLockConcurrentQueue<TData> : IConcurrentQueue<TData>
    {
        private readonly int _maxCapacity;
        private readonly IQueue<TData> _queue;
        
        public int Count
        {
            get { lock (_queue) { return _queue.Count; } }
        }

        public SynchronizedByLockConcurrentQueue(IQueue<TData> queue, int maxCapacity = -1)
        {
            _maxCapacity = (maxCapacity > 0) ? maxCapacity : -1;
            _queue = queue;
        }

        public bool Put(TData value)
        {
            lock (_queue)
            {
                if (_maxCapacity == -1 || _queue.Count < _maxCapacity)
                {
                    return _queue.Put(value);
                }
                return false;
            }
        }

        public bool TryPop([MaybeNullWhen(false)] out TData value)
        {
            lock (_queue)
            {
                return _queue.TryPop(out value);
            }
        }
    }
}
