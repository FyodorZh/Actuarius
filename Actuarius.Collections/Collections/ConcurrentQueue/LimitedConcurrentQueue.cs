using System;
using System.Diagnostics.CodeAnalysis;

namespace Actuarius.Collections
{
    /// <summary>
    /// TODO: Don't use it. It is slow and possibly buggy implementation of limited concurrent queue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LimitedConcurrentQueue<T> : IConcurrentQueue<T>
    {
        private struct Element
        {
            public T Data;
            public long Flag;
        }

        private readonly int _capacity;
        private readonly Element[] _data;

        private long _start = 1;
        private long _end;

        private int _count;
        
        public int Count => Math.Min(_count, _capacity - 1);

        public LimitedConcurrentQueue(int capacity)
        {
            _capacity = capacity;
            _data = new Element[_capacity];
        }

        public bool Put(T value)
        {
            int count = System.Threading.Interlocked.Increment(ref _count);
            if (count <= _capacity)
            {
                System.Threading.Thread.MemoryBarrier();
                long pos = System.Threading.Interlocked.Increment(ref _end);
                int posToWrite = (int)(pos % _capacity);
                _data[posToWrite].Data = value;
                System.Threading.Thread.MemoryBarrier(); // вроде как запись реордериться не должна ???
                System.Threading.Interlocked.Exchange(ref _data[posToWrite].Flag, pos);
                return true;
            }

            System.Threading.Interlocked.Decrement(ref _count);
            return false;
        }

        public bool TryPop([MaybeNullWhen(false)] out T value)
        {
            while (true)
            {
                long start = System.Threading.Interlocked.Read(ref _start);
                if (start > System.Threading.Interlocked.Read(ref _end))
                {
                    value = default;
                    return false;
                }
                int posToRead = (int)(start % _capacity);

                if (System.Threading.Interlocked.CompareExchange(ref _data[posToRead].Flag, 0, start) == start)
                {
                    System.Threading.Thread.MemoryBarrier();
                    value = _data[posToRead].Data;
                    _data[posToRead].Data = default!;
                    System.Threading.Interlocked.Increment(ref _start);
                    System.Threading.Interlocked.Decrement(ref _count);
                    return true;
                }
            }
        }
    }
}
