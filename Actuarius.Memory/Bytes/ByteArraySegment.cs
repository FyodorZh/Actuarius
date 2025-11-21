using System;

namespace Actuarius.Memory
{
    public readonly struct ByteArraySegment_new : IByteArray
    {
        private readonly byte[] _array;
        private readonly int _offset;
        private readonly int _count;

        public ByteArraySegment_new(byte[] array)
            : this(array, 0, array.Length)
        {
        }

        public ByteArraySegment_new(byte[] array, int offset, int count)
        {
            _array = array;
            _offset = offset;
            _count = count;
        }

        public bool IsValid
        {
            get
            {
                if (_array != null)
                {
                    int len = _array.Length;
                    return _offset >= 0 && _offset <= len && _count >= 0 && _offset + _count <= len;
                }
                return false;
            }
        }

        public byte this[int id]
        {
            get => _array[_offset + id];
            set => _array[_offset + id] = value;
        }

        public byte[] ReadOnlyArray => _array;

        public byte[] Array => _array;
        public int Offset => _offset;
        public int Count => _count;

        public byte[] ToByteArray()
        {
            if (IsValid)
            {
                byte[] res = new byte[_count];
                System.Buffer.BlockCopy(_array, _offset, res, 0, res.Length);
                return res;
            }

            throw new NullReferenceException();
        }

        public ByteArraySegment_new TrimLeft(int count)
        {
            if (!IsValid)
            {
                return new ByteArraySegment_new();
            }

            if (count > _count)
            {
                count = _count;
            }
            if (count < 0)
            {
                count = 0;
            }

            return new ByteArraySegment_new(_array, _offset + count, _count - count);
        }

        public ByteArraySegment_new Sub(int offset, int count)
        {
            if (!IsValid || offset < 0 || count < 0)
            {
                return new ByteArraySegment_new();
            }

            if (_count < offset)
            {
                return new ByteArraySegment_new();
            }

            count = Math.Min(count, _count - offset);
            return new ByteArraySegment_new(_array, _offset + offset, count);
        }

        public override bool Equals(object? obj)
        {
            if (obj is ByteArraySegment_new other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(ByteArraySegment_new obj)
        {
            return _array == obj._array &&
                   _offset == obj._offset;
        }

        public override int GetHashCode()
        {
            if (_array != null)
            {
                return _array.GetHashCode() ^ _offset ^ _count;
            }
            return 0;
        }

        public static bool operator ==(ByteArraySegment_new a, ByteArraySegment_new b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ByteArraySegment_new a, ByteArraySegment_new b)
        {
            return !a.Equals(b);
        }

        public bool EqualByContent(ByteArraySegment_new data)
        {
            if (!IsValid || !data.IsValid)
            {
                return IsValid == data.IsValid;
            }

            if (_count != data._count)
            {
                return false;
            }

            // todo optimize

            int count = _count;
            for (int i = 0; i < count; ++i)
            {
                if (_array[_offset + i] != data._array[data._offset + i])
                {
                    return false;
                }
            }

            return true;
        }

        public bool CopyTo(byte[] dst, int offset)
        {
            return CopyTo(dst, offset, 0, _count);
        }

        public bool CopyTo(byte[] dst, int offset, int srcOffset, int count)
        {
            if (IsValid)
            {
                if (count < 0 || count > _count)
                {
                    return false;
                }

                if (srcOffset < 0 || srcOffset + count > _count)
                {
                    return false;
                }

                if (offset >= 0 && offset <= dst.Length)
                {
                    if (dst.Length - offset < count)
                    {
                        return false;
                    }

                    System.Buffer.BlockCopy(_array, _offset + srcOffset, dst, offset, count);
                    return true;
                }
            }
            return false;
        }

        public bool CopyFrom(int dstPosition, byte[] src, int srcOffset, int count)
        {
            if (IsValid)
            {
                if (srcOffset >= 0 && count > 0 && srcOffset + count <= src.Length)
                {
                    if (dstPosition >= 0 && dstPosition + count <= _count)
                    {
                        Buffer.BlockCopy(src, srcOffset, _array, _offset + dstPosition, count);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}