using Actuarius.Collections;

namespace Actuarius.Memory
{
    public abstract class Pool<TResource, TParam0> : IPool<TResource, TParam0>
        where TResource : class
    {
        private readonly IMap<int, IPool<TResource>> _table;

        protected abstract IPool<TResource> CreatePool(int classId);
        
        protected abstract int Classify(TResource resource);
        
        protected abstract  int Classify(TParam0 param0);

        protected Pool(IMap<int, IPool<TResource>> table)
        {
            _table = table;
        }
        
        public void Release(TResource? resource)
        {
            if (resource != null)
            {
                int classId = Classify(resource);
                if (_table.TryGetValue(classId, out var subPool))
                {
                    subPool.Release(resource);
                }
                else
                {
                    //Log.e("Error!!!");
                    // TODO
                }
            }
        }

        public TResource Acquire(TParam0 param0)
        {
            int classId = Classify(param0);

            if (!_table.TryGetValue(classId, out var subPool))
            {
                subPool = CreatePool(classId);
                _table.Add(classId, subPool);
            }

            return subPool.Acquire();
        }

        public (TResource resource, IPoolSink<TResource> poolSink) AcquireEx(TParam0 param0)
        {
            int classId = Classify(param0);

            if (!_table.TryGetValue(classId, out var subPool))
            {
                subPool = CreatePool(classId);
                _table.Add(classId, subPool);
            }

            return (subPool.Acquire(), subPool);
        }
    }
}