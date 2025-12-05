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
        
        private readonly Func<object>?[] _constructors = new Func<object>?[MaxTypeId];
        private readonly CollectablePoolCore _pool;
           
        public CollectablePool(Func<IConcurrentUnorderedCollection<object>> singleTypePool)
        {
            _pool = new CollectablePoolCore(_constructors, singleTypePool);
        }
        
        public TResource Acquire<TResource>() where TResource : class, ICollectableResource<TResource>, new()
        {
            int typeId = TypeMap<TResource>.TypeId;
            _constructors[typeId] ??= () => new TResource();
            
            (object res, IPoolSink<object> poolSink) = _pool.AcquireEx(typeId);
            var resource = (TResource)res;
            resource.Restored(poolSink);
            return resource;
        }

        private class CollectablePoolCore : ConcurrentPool<object, int>
        {
            private readonly Func<object>?[] _constructors;
            private readonly Func<IConcurrentUnorderedCollection<object>> _concurrentQueueFactory;
            
            public CollectablePoolCore(Func<object>?[] constructors, Func<IConcurrentUnorderedCollection<object>> concurrentQueueFactory) 
                : base(new SynchronizedConcurrentDictionary<int, IConcurrentPool<object>>())
            {
                _constructors = constructors;
                _concurrentQueueFactory = concurrentQueueFactory;
            }

            protected override IConcurrentPool<object> CreatePool(int typeId)
            {
                return new ConcurrentDelegatePool<object>(
                    _constructors[typeId] ?? throw new Exception("Collectable pool internal error"), 
                    _concurrentQueueFactory.Invoke());
            }

            protected override int Classify(object resource)
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