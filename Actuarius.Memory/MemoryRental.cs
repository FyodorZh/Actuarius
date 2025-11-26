namespace Actuarius.Memory
{
    public interface IMemoryRental
    {
        IConcurrentPool<byte[], int> BytesPool { get; }
        IConcurrentPool<IMultiRefByteArray, int> ByteArraysPool { get; }
        ICollectablePool CollectablePool { get; }
    }

    public class MemoryRental : IMemoryRental
    {
        public static readonly IMemoryRental Shared = new MemoryRental();
        
        public IConcurrentPool<byte[], int> BytesPool { get; }
        public IConcurrentPool<IMultiRefByteArray, int> ByteArraysPool { get; }
        public ICollectablePool CollectablePool { get; }

        public MemoryRental()
        {
            BytesPool = new RawByteArrayConcurrentPool(size =>
            {
                if (size <= 1024)
                {
                    return 1000;
                }

                if (size <= 1024 * 10)
                {
                    return 100;
                }

                if (size <= 1024 * 128)
                {
                    return 10;
                }

                if (size <= 1024 * 1024)
                {
                    return 1;
                }

                return 0;
            });
            ByteArraysPool = new ByteArrayConcurrentPool(BytesPool);
            CollectablePool = new CollectablePool(100);
        }
    }
}