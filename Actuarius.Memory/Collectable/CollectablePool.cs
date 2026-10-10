using System;
using System.Threading;
using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class CollectablePool : ICollectablePool
    {
        private const int MaxTypeId = 1000;
        private static int _nextTypeId;
        
        // ReSharper disable once UnusedTypeParameter
        private static class TypeMap<TResource>
        {
            // ReSharper disable once StaticMemberInGenericType
            public static readonly int TypeId = Interlocked.Increment(ref _nextTypeId); 
        }
        
        private readonly Func<ICollectableResource>?[] _constructors = new Func<ICollectableResource>?[MaxTypeId + 1];
        private readonly CollectablePoolCore _pool;
           
        public CollectablePool(Func<IConcurrentUnorderedCollection<ICollectableResource>> singleTypePool)
        {
            _pool = new CollectablePoolCore(_constructors, singleTypePool);
        }
        
        public TResource Acquire<TResource>() where TResource : class, ICollectableResource<TResource>, new()
        {
            int typeId = TypeMap<TResource>.TypeId;
            _constructors[typeId] ??= () => new TResource();
            
            (object res, IPoolSink<ICollectableResource> poolSink) = _pool.AcquireEx(typeId);
            var resource = (TResource)res;
            resource.Restored(poolSink);
            return resource;
        }

        private class CollectablePoolCore : ConcurrentPool<ICollectableResource, int>
        {
            private readonly Func<ICollectableResource>?[] _constructors;
            private readonly Func<IConcurrentUnorderedCollection<ICollectableResource>> _concurrentQueueFactory;
            
            public CollectablePoolCore(Func<ICollectableResource>?[] constructors, Func<IConcurrentUnorderedCollection<ICollectableResource>> concurrentQueueFactory) 
                : base(new SynchronizedConcurrentDictionary<int, IConcurrentPool<ICollectableResource>>())
            {
                _constructors = constructors;
                _concurrentQueueFactory = concurrentQueueFactory;
            }

            protected override IConcurrentPool<ICollectableResource> CreatePool(int typeId)
            {
                return new ConcurrentDelegatePool<ICollectableResource>(
                    _constructors[typeId] ?? throw new Exception("Collectable pool internal error"), 
                    _concurrentQueueFactory.Invoke(),
                    collectable =>
                    {
                        try
                        {
                            collectable.Collected();
                            return true;
                        }
                        catch 
                        {
                            return false;
                        }
                    });
            }

            protected override int Classify(ICollectableResource resource)
            {
                throw new InvalidOperationException("This method must not be called.");
            }

            protected override int Classify(int param)
            {
                return param;
            }
        }
    }
}