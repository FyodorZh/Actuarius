using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Actuarius.Collections
{
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

        public int Count => Volatile.Read(ref _count);

        public LimitedConcurrentQueue(int capacity)
        {
            _capacity = capacity;
            _data = new Element[_capacity];
        }

        public bool Put(T value)
        {
            int count = Interlocked.Increment(ref _count);
            if (count <= _capacity)
            {
                long pos = Interlocked.Increment(ref _end);
                int posToWrite = (int)(pos % _capacity);
                _data[posToWrite].Data = value;

                Interlocked.Exchange(ref _data[posToWrite].Flag, pos);
                return true;
            }

            Interlocked.Decrement(ref _count);
            return false;
        }

        public bool TryPop([MaybeNullWhen(false)] out T value)
        {
            while (true)
            {
                long start = Volatile.Read(ref _start);
                long end = Volatile.Read(ref _end);

                if (start > end)
                {
                    value = default;
                    return false;
                }

                int posToRead = (int)(start % _capacity);

                if (Interlocked.CompareExchange(ref _data[posToRead].Flag, 0, start) == start)
                {
                    value = _data[posToRead].Data;
                    _data[posToRead].Data = default!;

                    Interlocked.Increment(ref _start);
                    Interlocked.Decrement(ref _count);
                    return true;
                }
            }
        }
    }
}