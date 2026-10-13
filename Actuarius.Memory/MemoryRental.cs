using Actuarius.Collections;

namespace Actuarius.Memory
{
    public interface IMemoryRental
    {
        IConcurrentPool<IMultiRefByteArray, int> ByteArraysPool { get; }
        IConcurrentPool<IMultiRefByteArray, int> SharedByteArraysPool { get; }
        
        ICollectablePool CollectablePool { get; }
        IGenericConcurrentPool SmallObjectsPool { get; }
        IGenericConcurrentPool BigObjectsPool { get; }
        IConcurrentPool<T[], int> GetArrayPool<T>();
    }

    public class MemoryRental : IMemoryRental
    {
        public static readonly IMemoryRental Shared = new MemoryRental();
            
        public IConcurrentPool<IMultiRefByteArray, int> ByteArraysPool { get; }
        public IConcurrentPool<IMultiRefByteArray, int> SharedByteArraysPool { get; }
        public ICollectablePool CollectablePool { get; }

        public IGenericConcurrentPool SmallObjectsPool { get; }
        public IGenericConcurrentPool BigObjectsPool { get; }

        private readonly IConcurrentMap<int, IPoolRef> _arrayPoolsMap = new SynchronizedConcurrentDictionary<int, IPoolRef>();
        
        public IConcurrentPool<T[], int> GetArrayPool<T>()
        {
            int typeId = TypeToIntStaticMap.GetTypeId<T>();
            if (!_arrayPoolsMap.TryGetValue(typeId, out IPoolRef? poolRef))
            {
                _arrayPoolsMap.AddOrGet(typeId, new RawArrayConcurrentPool<T>(size =>
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
                }), out poolRef);
            }

            return ((IConcurrentPool<T[], int>)poolRef!);
        }

        public MemoryRental()
        {
            var bytesPool = GetArrayPool<byte>();
            ByteArraysPool = new ByteArrayConcurrentPool(bytesPool);
            SharedByteArraysPool = new ByteArrayConcurrentPool(bytesPool, 1000);
            
            CollectablePool = new CollectablePool(() => new LimitedConcurrentQueue<ICollectableResource>(100));

            SmallObjectsPool = new GenericConcurrentPool(new SynchronizedConcurrentDictionary<int, object>(), 100, 10);
            BigObjectsPool = new GenericConcurrentPool(new SynchronizedConcurrentDictionary<int, object>(), 10, 2);
        }
    }
}