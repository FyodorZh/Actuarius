namespace Actuarius.Memory
{
    public class StaticByteArray : StaticReadOnlyByteArray, IMultiRefByteArray
    {
        public StaticByteArray(byte[] array, int offset, int length)
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