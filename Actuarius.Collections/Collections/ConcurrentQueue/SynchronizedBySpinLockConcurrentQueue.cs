using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Actuarius.Collections
{
    public class SynchronizedBySpinLockConcurrentQueue<TData> : IConcurrentQueue<TData>
    {
        private readonly int _maxCapacity;
        private readonly IQueue<TData> _queue;

        private SpinLock _spinLock;
        
        public int Count
        {
            get 
            { 
                bool gotLock = false;
                try
                {
                    _spinLock.Enter(ref gotLock);
                    return _queue.Count;
                }
                finally
                {
                    if (gotLock)
                        _spinLock.Exit();
                }
            }
        }

        public SynchronizedBySpinLockConcurrentQueue(IQueue<TData> queue, int maxCapacity = -1)
        {
            _maxCapacity = (maxCapacity > 0) ? maxCapacity : -1;
            _queue = queue;
            _spinLock = new SpinLock();
        }

        public bool Put(TData value)
        {
            bool gotLock = false;
            try
            {
                _spinLock.Enter(ref gotLock);
                if (_maxCapacity == -1 || _queue.Count < _maxCapacity)
                {
                    return _queue.Put(value);
                }
                return false;
            }
            finally
            {
                if (gotLock)
                    _spinLock.Exit();
            }
        }

        public bool TryPop([MaybeNullWhen(false)] out TData value)
        {
            bool gotLock = false;
            try
            {
                _spinLock.Enter(ref gotLock);
                return _queue.TryPop(out value);
            }
            finally
            {
                if (gotLock)
                    _spinLock.Exit();
            }
        }
    }
}
