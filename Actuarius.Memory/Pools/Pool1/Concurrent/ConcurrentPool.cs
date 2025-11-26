using Actuarius.Collections;

namespace Actuarius.Memory
{
    public abstract class ConcurrentPool<TResource, TParam> : IConcurrentPool<TResource, TParam>
        where TResource : class
    {
        private readonly IConcurrentMap<int, IConcurrentPool<TResource>> _table;

        protected abstract IConcurrentPool<TResource> CreatePool(int classId);
        
        protected abstract int Classify(TResource resource);
        
        protected abstract int Classify(TParam param);

        protected ConcurrentPool(IConcurrentMap<int, IConcurrentPool<TResource>> table)
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

        public TResource Acquire(TParam param)
        {
            int classId = Classify(param);

            if (!_table.TryGetValue(classId, out var subPool))
            {
                subPool = CreatePool(classId);
                _table.Add(classId, subPool);
            }

            return subPool.Acquire();
        }
        
        public (TResource resource, IConcurrentPool<TResource> subPool) AcquireEx(TParam param)
        {
            int classId = Classify(param);

            if (!_table.TryGetValue(classId, out var subPool))
            {
                subPool = CreatePool(classId);
                _table.Add(classId, subPool);
            }

            return (subPool.Acquire(), subPool);
        }
    }
}