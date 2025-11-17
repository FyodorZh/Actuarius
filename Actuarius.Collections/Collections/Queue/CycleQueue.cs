using System;
using System.Diagnostics.CodeAnalysis;

namespace Actuarius.Collections
{
    public class CycleQueue<T> : IQueue<T>, IArray<T>
    {
        private readonly bool _canGrow;

        private int _capacity; // Всегда степень двойки
        private int _capacityMask;

        private int _count;
        private int _position;

        private T[] _data;

        public CycleQueue()
            : this(16)
        {
        }

        public CycleQueue(int capacity, bool canGrow = true)
        {
            _capacity = BitMath.NextPow2((uint)capacity);
            _capacityMask = _capacity - 1;

            _count = 0;
            _position = 0;
            _data = new T[_capacity];
            _canGrow = canGrow;
        }

        public void Clear()
        {
            _count = 0;
        }

        public int Capacity => _capacity;

        public int Count => _count;

        private bool Grow()
        {
            if (_count == _capacity)
            {
                if (_canGrow)
                {
                    T[] newData = new T[_capacity * 2];

                    ArrayCopier<T>.Copy(_data, _position, newData, 0, _capacity - _position);
                    if (_position != 0)
                    {
                        ArrayCopier<T>.Copy(_data, 0, newData, _capacity - _position, _position);
                    }

                    _data = newData;

                    _capacity *= 2;
                    _capacityMask = _capacity - 1;

                    _position = 0;
                    return true;
                }

                return false;
            }

            return true;
        }

        public bool Put(T value)
        {
            if (Grow())
            {
                _data[(_position + _count) & _capacityMask] = value;
                _count += 1;
                return true;
            }

            return false;
        }

        public bool PutToHead(T value)
        {
            if (Grow())
            {
                _position = (_position + _capacity - 1) & _capacityMask;
                _data[_position] = value;
                _count += 1;
                return true;
            }

            return false;
        }

        public bool TryPop([MaybeNullWhen(false)] out T value)
        {
            if (_count > 0)
            {
                value = _data[_position];
                _data[_position] = default!;
                _count -= 1;
                _position = (_position + 1) & _capacityMask;
                return true;
            }
            value = default;
            return false;
        }

        public T Head
        {
            get
            {
                if (_count <= 0)
                {
                    throw new Exception();
                }
                return _data[_position];
            }
        }

        public T Tail
        {
            get
            {
                if (_count <= 0)
                {
                    throw new Exception();
                }
                return _data[(_position + _count - 1) & _capacityMask];
            }
        }

        public T this[int id]
        {
            get
            {
                if (id >= 0 && id < _count)
                {
                    return _data[(_position + id) & _capacityMask];
                }
                throw new Exception();
            }
            set
            {
                if (id >= 0 && id < _count)
                {
                    _data[(_position + id) & _capacityMask] = value;
                    return;
                }
                throw new Exception();
            }
        }

        public EnumerableWithOrder Enumerate()
        {
            return new EnumerableWithOrder(this, QueueEnumerationOrder.HeadToTail);
        }

        public EnumerableWithOrder Enumerate(QueueEnumerationOrder order)
        {
            return new EnumerableWithOrder(this, order);
        }

        public readonly struct EnumerableWithOrder
        {
            private readonly CycleQueue<T> _queue;
            private readonly QueueEnumerationOrder _order;

            internal EnumerableWithOrder(CycleQueue<T> queue, QueueEnumerationOrder order)
            {
                _queue = queue;
                _order = order;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(_queue, _order);
            }
        }

        public struct Enumerator
        {
            private readonly CycleQueue<T> _queue;
            private readonly QueueEnumerationOrder _order;
            private int _current;
            private int _end;

            internal Enumerator(CycleQueue<T> queue, QueueEnumerationOrder order)
            {
                _queue = queue;
                _order = order;
                _current = 0;
                _end = 0;
                SetIndices();
            }

            public void Dispose()
            {
                // DO NOTHING
            }

            private void SetIndices()
            {
                switch (_order)
                {
                    case QueueEnumerationOrder.HeadToTail:
                        _current = _queue._position - 1;
                        _end = _queue._position + _queue._count - 1;
                        break;
                    case QueueEnumerationOrder.TailToHead:
                        _current = _queue._position + _queue._count;
                        _end = _queue._position;
                        break;
                    default:
                        _current = 0;
                        _end = 0;
                        break;
                }
            }

            public bool MoveNext()
            {
                switch (_order)
                {
                    case QueueEnumerationOrder.HeadToTail:
                        if (_current == _end)
                        {
                            return false;
                        }
                        _current++;
                        return true;
                    case QueueEnumerationOrder.TailToHead:
                        if (_current == _end)
                        {
                            return false;
                        }
                        _current--;
                        return true;
                    default:
                        return false;
                }
            }

            public T Current => _queue._data[_current & _queue._capacityMask];
        }

        // [UT.UT("CycleQueue")]
        // private static void UT(UT.IUTest test)
        // {
        //     CycleQueue<int> queue = new CycleQueue<int>(2);
        //     test.Equal(queue.Count, 0);
        //
        //     int output;
        //
        //     queue.Put(1);
        //     test.Equal(queue.Count, 1);
        //     test.Equal(queue.Head, queue.Tail);
        //     test.Equal(queue.Tail, queue[0]);
        //
        //     test.Equal(queue.TryPop(out output), true);
        //     test.Equal(output, 1);
        //
        //     queue.Put(1);
        //     queue.Put(2);
        //     test.Equal(queue.Count, 2);
        //     test.Equal(queue.Head, 1);
        //     test.Equal(queue.Tail, 2);
        //     test.Equal(queue[0], 1);
        //     test.Equal(queue[1], 2);
        //
        //     queue.Put(3);
        //
        //     Log.d("HEAD TO TAIL");
        //     foreach (var q in queue.Enumerate(QueueEnumerationOrder.HeadToTail))
        //     {
        //         Log.d("{0}", q);
        //     }
        //
        //     Log.d("TAIL TO HEAD");
        //     foreach (var q in queue.Enumerate(QueueEnumerationOrder.TailToHead))
        //     {
        //         Log.d("{0}", q);
        //     }
        //
        //     test.Equal(queue[2], 3);
        //
        //     test.Equal(queue.TryPop(out output), true);
        //     test.Equal(output, 1);
        //
        //     test.Equal(queue[0], 2);
        //
        //     test.Equal(queue.TryPop(out output), true);
        //     test.Equal(output, 2);
        //
        //     test.Equal(queue.TryPop(out output), true);
        //     test.Equal(output, 3);
        // }

    }
}
