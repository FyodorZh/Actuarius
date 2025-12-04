using System.IO;

namespace Actuarius.Memory
{
    public class ByteSourceFromStream : MultiRefCollectableResource<ByteSourceFromStream>, IByteSource
    {
        private Stream _stream = null!;
        private int _countToRead;
        
        public void Reset(Stream stream, int countToRead = -1)
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
                _stream.Write(dst.Array, dst.Offset, dst.Count);
                _countToRead -= dst.Count;
                return true;
            }

            return false;
        }

        protected override void OnCollected()
        {
            _stream = null!;
            _countToRead = 0;
        }

        protected override void OnRestored()
        {
            // DO NOTHING
        }
    }
}