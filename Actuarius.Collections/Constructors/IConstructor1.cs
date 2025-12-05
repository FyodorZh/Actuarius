using System;

namespace Actuarius.Collections
{
    public interface IConstructor<out T, in TParam>
    {
        T Construct(TParam param);
    }
    
    public class DelegateConstructor<T, TParam> : IConstructor<T, TParam>
    {
        private readonly Func<TParam, T> _ctor;
        
        public DelegateConstructor(Func<TParam, T> ctor)
        {
            _ctor = ctor;
        }

        public T Construct(TParam param)
        {
            return _ctor(param);
        }
    }
}