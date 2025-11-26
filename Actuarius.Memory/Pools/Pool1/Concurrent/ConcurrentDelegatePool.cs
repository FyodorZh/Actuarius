using System;
using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class ConcurrentDelegatePool<TResource, TParam> : ConcurrentPool<TResource, TParam>
        where TResource : class
    {
        private readonly Func<int, IConcurrentPool<TResource>> _poolCtor;
        private readonly Func<TResource, int> _classifyByResource;
        private readonly Func<TParam, int> _classifyByParameter;

        public ConcurrentDelegatePool(IConcurrentMap<int, IConcurrentPool<TResource>> table, 
            Func<int, IConcurrentPool<TResource>> poolCtor, 
            Func<TResource, int> classifyByResource, 
            Func<TParam, int> classifyByParameter)
            :base(table)
        {
            _poolCtor = poolCtor;
            _classifyByResource = classifyByResource;
            _classifyByParameter = classifyByParameter;
        }

        protected override IConcurrentPool<TResource> CreatePool(int classId)
        {
            return _poolCtor(classId);
        }

        protected override int Classify(TResource resource)
        {
            return _classifyByResource(resource);
        }

        protected override int Classify(TParam param)
        {
            return _classifyByParameter(param);
        }
    }
}