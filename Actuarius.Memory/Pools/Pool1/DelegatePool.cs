using System;
using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class DelegatePool<TResource, TParam1> : Pool<TResource, TParam1>
        where TResource : class
    {
        private readonly Func<int, IPool<TResource>> _poolConstructor;
        private readonly Func<TResource, int> _classifyByResource;
        private readonly Func<TParam1, int> _classifyByParameter;

        public DelegatePool(IMap<int, IPool<TResource>> table, Func<int, IPool<TResource>> poolCtor, Func<TResource, int> classifyByResource, Func<TParam1, int> classifyByParameter)
            :base(table)
        {
            _poolConstructor = poolCtor;
            _classifyByResource = classifyByResource;
            _classifyByParameter = classifyByParameter;
        }

        protected override IPool<TResource> CreatePool(int classId)
        {
            return _poolConstructor(classId);
        }

        protected override int Classify(TResource resource)
        {
            return _classifyByResource(resource);
        }
        
        protected override int Classify(TParam1 param0)
        {
            return _classifyByParameter(param0);
        }
    }
}