using System;
using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class GenericConcurrentPool : IGenericConcurrentPool
    {
        private readonly IConcurrentMap<int, object> _typeIdMap; // object == IConcurrentPool<TResource>

        private readonly int _bucketSize;
        private readonly int _distributionLevel;
           
        public GenericConcurrentPool(IConcurrentMap<int, object> typeIdMap, int bucketSize, int distributionLevel)
        {
            _typeIdMap = typeIdMap;
            _bucketSize = bucketSize;
            _distributionLevel = distributionLevel;
        }

        IPool<TResource> IGenericPool.GetPool<TResource>()
        {
            return GetPool<TResource>();
        }

        public IConcurrentPool<TResource> GetPool<TResource>()
            where TResource : class, new()
        {
            int typeId = TypeToIntStaticMap.GetTypeId<TResource>();

            if (!_typeIdMap.TryGetValue(typeId, out var pool))
            {
                var newPool = new BufferedPool<TResource>(_bucketSize, _distributionLevel, () => new TResource());
                if (!_typeIdMap.AddOrGet(typeId, newPool, out pool))
                {
                    throw new Exception("Type map capacity exceeded");
                }
            }

            return (IConcurrentPool<TResource>)pool;
        }

        public (TResource resource, IPoolSink<TResource> poolSink) Acquire<TResource>()
            where TResource : class, new()
        {
            var pool = GetPool<TResource>();
            return (pool.Acquire(), pool);
        }
    }
}