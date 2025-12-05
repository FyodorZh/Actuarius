using System;
using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class GenericConcurrentPool<TResourceRestriction, TParam> : IGenericConcurrentPool<TResourceRestriction, TParam>
    {
        private readonly IConcurrentMap<int, IPoolRef> _typeIdMap; // IPoolRef == IConcurrentPool<TResource>

        private readonly IGenericConstructor<IPoolRef> _poolConstructor;
           
        public GenericConcurrentPool(IConcurrentMap<int, IPoolRef> typeIdMap, IGenericConstructor<IPoolRef> poolConstructor)
        {
            _typeIdMap = typeIdMap;
            _poolConstructor = poolConstructor;
        }

        IPool<TResource, TParam> IGenericPool<TResourceRestriction, TParam>.ShowTypedPool<TResource>()
        {
            return ShowTypedPool<TResource>();
        }

        public IConcurrentPool<TResource, TParam> ShowTypedPool<TResource>()
        {
            int typeId = TypeToIntStaticMap.GetTypeId<TResource>();

            if (!_typeIdMap.TryGetValue(typeId, out var pool))
            {
                var newPool = _poolConstructor.Construct<IConcurrentPool<TResource>>();
                if (!_typeIdMap.AddOrGet(typeId, newPool, out pool))
                {
                    throw new Exception("Type map capacity exceeded");
                }
            }

            return (IConcurrentPool<TResource, TParam>)pool;
        }

        public (TResource resource, IPoolSink<TResource> poolSink) Acquire<TResource>(TParam param) where TResource : TResourceRestriction
        {
            var pool = ShowTypedPool<TResource>();
            return pool.AcquireEx(param);
        }
    }
}