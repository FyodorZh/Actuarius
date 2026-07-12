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

        // Fix 3: Count now returns the actual number of items, never exceeding _capacity.
        // Volatile.Read ensures a fresh value with acquire semantics.
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
                // Fix 4: Removed the unnecessary Thread.MemoryBarrier() here.
                // Interlocked.Increment already provides a full fence.

                long pos = Interlocked.Increment(ref _end);
                int posToWrite = (int)(pos % _capacity);
                _data[posToWrite].Data = value;

                // Fix 4: Removed the barrier before the Exchange.
                // Interlocked.Exchange is a full fence and guarantees that the Data write
                // is visible to any thread that later sees this Flag value.
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
                // Fix 2: Use Volatile.Read with acquire semantics to avoid seeing stale
                // values of _start and _end on weakly ordered hardware (e.g. ARM).
                long start = Volatile.Read(ref _start);
                long end = Volatile.Read(ref _end);

                if (start > end)
                {
                    value = default;
                    return false;
                }

                int posToRead = (int)(start % _capacity);

                // The CAS is a full fence, so if we succeed we have acquire/release.
                if (Interlocked.CompareExchange(ref _data[posToRead].Flag, 0, start) == start)
                {
                    // Fix 4: The CAS already provides a full memory barrier.
                    // The following read of Data is therefore correctly ordered.
                    value = _data[posToRead].Data;
                    _data[posToRead].Data = default!;

                    Interlocked.Increment(ref _start);
                    Interlocked.Decrement(ref _count);
                    return true;
                }
                // If the CAS fails, the slot is either not ready or already taken – retry.
            }
        }
    }
}