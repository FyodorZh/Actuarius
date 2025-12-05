using System;

namespace Actuarius.Collections
{
    public interface IConstructor<out T>
    {
        T Construct();
    }

    public class DefaultConstructor<T> : IConstructor<T>
        where T : new()
    {
        public static readonly DefaultConstructor<T> Instance = new();
        
        public T Construct()
        {
            return new();
        }
    }

    public class DelegateConstructor<T> : IConstructor<T>
    {
        private readonly Func<T> _ctor;
        
        public DelegateConstructor(Func<T> ctor)
        {
            _ctor = ctor;
        }

        public T Construct()
        {
            return _ctor();
        }
    }
}