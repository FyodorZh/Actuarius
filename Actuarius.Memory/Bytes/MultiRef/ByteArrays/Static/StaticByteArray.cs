namespace Actuarius.Memory
{
    public class StaticByteArray : StaticReadOnlyByteArray, IMultiRefByteArray
    {
        public StaticByteArray(byte[] array, int offset = 0, int length = -1)
            : base(array, offset, length)
        {
        }

        public new byte this[int id]
        {
            get => _array[_offset + id];
            set => _array[_offset + id] = value;
        }

        public byte[] Array => _array;
    }
}