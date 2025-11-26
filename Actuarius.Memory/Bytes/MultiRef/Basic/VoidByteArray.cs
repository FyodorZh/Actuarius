using System;

namespace Actuarius.Memory
{
    /// <summary>
    /// Эквивалентно default(byte[])
    /// Является синглтоном. Позволяет возвращать невалидные массивы без аллокаций
    /// </summary>
    public class VoidByteArray : MultiRefByteArray
    {
        public static readonly MultiRefByteArray Instance = new VoidByteArray();

        private VoidByteArray()
            : base(null!, 0, 0)
        {
        }

        public override bool CopyTo(byte[] dst, int dstOffset, int srcOffset, int count)
        {
            return false;
        }
    }
}