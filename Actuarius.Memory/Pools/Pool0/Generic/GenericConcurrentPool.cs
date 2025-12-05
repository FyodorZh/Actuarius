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
        
        public (TResource resource, IPoolSink<TResource> poolSink) Acquire<TResource>() where TResource : class, new()
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

            var pool0 = (IConcurrentPool<TResource>)pool;

            return (pool0.Acquire(), pool0);
        }
    }
}