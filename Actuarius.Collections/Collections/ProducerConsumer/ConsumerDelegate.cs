using System;

namespace Actuarius.Collections
{
    public struct ConsumerDelegate<TData> : IConsumer<TData>
    {
        private readonly Func<TData, bool> _action;
        
        public ConsumerDelegate(Func<TData, bool> action)
        {
            _action = action;
        }
        
        public bool Put(TData value)
        {
            return _action.Invoke(value);
        }
    }
    
    public struct ConcurrentConsumerDelegate<TData> : IConcurrentConsumer<TData>
    {
        private readonly Func<TData, bool> _action;
        
        public ConcurrentConsumerDelegate(Func<TData, bool> action)
        {
            _action = action;
        }
        
        public bool Put(TData value)
        {
            return _action.Invoke(value);
        }
    }
}