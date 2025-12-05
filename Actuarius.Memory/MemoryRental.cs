using System;
using Actuarius.Collections;

namespace Actuarius.Memory
{
    public interface IMemoryRental
    {
        IGenericConcurrentPool<Array, int> ArrayPool { get; }
        IConcurrentPool<IMultiRefByteArray, int> ByteArraysPool { get; }
        ICollectablePool CollectablePool { get; }
        IGenericConcurrentPool SmallObjectsPool { get; }
        IGenericConcurrentPool BigObjectsPool { get; }
    }

    public class MemoryRental : IMemoryRental
    {
        public static readonly IMemoryRental Shared = new MemoryRental();
        
        public IGenericConcurrentPool<Array, int> ArrayPool { get; }
            
        public IConcurrentPool<IMultiRefByteArray, int> ByteArraysPool { get; }
        public ICollectablePool CollectablePool { get; }

        public IGenericConcurrentPool SmallObjectsPool { get; }
        public IGenericConcurrentPool BigObjectsPool { get; }

        private class ArrayPoolCtor : IGenericConstructor<IPoolRef>
        {
            T IGenericConstructor<IPoolRef>.Construct<T>()
            {
                return (T)(IPoolRef)new RawArrayConcurrentPool<T>(size =>
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
            }
        }

        public MemoryRental()
        {
            ArrayPool = new GenericConcurrentPool<Array, int>(new SynchronizedConcurrentDictionary<int, IPoolRef>(), new ArrayPoolCtor());
            
            ByteArraysPool = new ByteArrayConcurrentPool(ArrayPool.ShowTypedPool<byte[]>());
            CollectablePool = new CollectablePool(() => new LimitedConcurrentQueue<object>(100));

            SmallObjectsPool = new GenericConcurrentPool(new SynchronizedConcurrentDictionary<int, object>(), 100, 10);
            BigObjectsPool = new GenericConcurrentPool(new SynchronizedConcurrentDictionary<int, object>(), 10, 2);
        }
    }
}