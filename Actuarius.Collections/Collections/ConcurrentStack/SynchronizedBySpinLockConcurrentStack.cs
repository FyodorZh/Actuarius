using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Actuarius.Collections
{
    public class SynchronizedBySpinLockConcurrentStack<TData> : IConcurrentStack<TData>
    {
        private readonly Stack<TData> _stack = new();
        private readonly int _maxCapacity;

        private SpinLock _spinLock;

        public int Count
        {
            get
            {
                bool lockTaken = false;
                try
                {
                    _spinLock.Enter(ref lockTaken);
                    return _stack.Count;
                }
                finally
                {
                    if (lockTaken)
                        _spinLock.Exit();
                }
            }
        }

        public SynchronizedBySpinLockConcurrentStack(int maxCapacity = -1)
        {
            _maxCapacity = maxCapacity;
            _spinLock = new SpinLock();
        }

        public bool Put(TData value)
        {
            bool lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);
                if (_maxCapacity == -1 || _stack.Count < _maxCapacity)
                {
                    _stack.Push(value);
                    return true;
                }

                return false;
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
        }

        public bool TryPop([MaybeNullWhen(false)] out TData value)
        {
            bool lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);
                if (_stack.Count > 0)
                {
                    value = _stack.Pop();
                    return true;
                }

                value = default;
                return false;
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
        }
    }
}