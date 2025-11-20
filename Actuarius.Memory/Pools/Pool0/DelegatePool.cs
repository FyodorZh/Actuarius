using System;
using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class DelegatePool<TObject> : IPool<TObject>
        where TObject : class
    {
        private readonly Func<TObject> _ctor;
        private readonly IUnorderedCollection<TObject> _pool;

        public DelegatePool(Func<TObject> ctor)
            : this(ctor, new CycleQueue<TObject>())
        {
        }

        public DelegatePool(Func<TObject> ctor, IUnorderedCollection<TObject> pool)
        {
            _ctor = ctor;
            _pool = pool;
        }

        public void Release(TObject? obj)
        {
            if (obj != null)
            {
                _pool.Put(obj);
            }
        }

        public TObject Acquire()
        {
            if (_pool.TryPop(out var obj))
            {
                return obj;
            }
            return _ctor.Invoke();
        }
    }
}