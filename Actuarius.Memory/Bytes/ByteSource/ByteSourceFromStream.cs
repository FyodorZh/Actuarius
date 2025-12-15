using System.IO;

namespace Actuarius.Memory
{
    public struct ByteSourceFromStream : IByteSource
    {
        private readonly Stream _stream;
        private int _countToRead;
        
        public ByteSourceFromStream(Stream stream, int countToRead = -1)
        {
            _stream = stream;
            _countToRead = countToRead < 0 ? (int)(stream.Length - stream.Position) : countToRead;
        }

        public bool TryPop(out byte value)
        {
            if (_countToRead > 0)
            {
                value = (byte)_stream.ReadByte();
                _countToRead -= 1;
                return true;
            }

            value = 0;
            return false;
        }

        public bool TakeMany(IMultiRefByteArray dst)
        {
            if (_countToRead >= dst.Count)
            {
                int actual = _stream.Read(dst.Array, dst.Offset, dst.Count);
                _countToRead -= actual;
                return actual == dst.Count;
            }

            return false;
        }
    }
}