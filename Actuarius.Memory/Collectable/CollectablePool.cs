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
           
        public CollectablePool(int capacity)
        {
            _pool = new CollectablePoolCore(_constructors, capacity);
        }
        
        public TResource Acquire<TResource>() 
            where TResource : class, ICollectableResource<TResource>, new()
        {
            int typeId = TypeMap<TResource>.TypeId;
            _constructors[typeId] ??= () => new TResource();
            
            (object res, IConcurrentPool<object> pool) = _pool.AcquireEx(typeId);
            var resource = (TResource)res;
            resource.Restored(pool);
            return resource;
        }

        private class CollectablePoolCore : ConcurrentPool<object, int>
        {
            private readonly Func<object>?[] _constructors;
            private readonly int _capacity;
            
            public CollectablePoolCore(Func<object>?[] constructors, int capacity) 
                : base(new SynchronizedConcurrentDictionary<int, IConcurrentPool<object>>())
            {
                _constructors = constructors;
                _capacity = capacity;
            }

            protected override IConcurrentPool<object> CreatePool(int typeId)
            {
                return new ConcurrentDelegatePool<object>(
                    _constructors[typeId] ?? throw new Exception("Collectable pool internal error"), 
                    new LimitedConcurrentQueue<object>(_capacity));
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