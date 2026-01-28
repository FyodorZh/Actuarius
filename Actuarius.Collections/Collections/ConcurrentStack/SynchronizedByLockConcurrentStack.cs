using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Actuarius.Collections
{
    public class SynchronizedByLockConcurrentStack<TData> : IConcurrentStack<TData>
    {
        private readonly Stack<TData> _stack = new();

        private readonly int _maxCapacity;

        public int Count
        {
            get
            {
                lock (_stack)
                {
                    return _stack.Count;
                }
            }
        }

        public SynchronizedByLockConcurrentStack(int maxCapacity = -1)
        {
            _maxCapacity = maxCapacity;
        }

        public bool Put(TData value)
        {
            lock (_stack)
            {
                if (_maxCapacity == -1 || _stack.Count < _maxCapacity)
                {
                    _stack.Push(value);
                    return true;
                }

                return false;
            }
        }

        public bool TryPop([MaybeNullWhen(false)] out TData value)
        {
            lock (_stack)
            {
                if (_stack.Count > 0)
                {
                    value = _stack.Pop();
                    return true;
                }

                value = default;
                return false;
            }
        }
    }
}