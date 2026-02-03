using System;
using System.Diagnostics.CodeAnalysis;

namespace Actuarius.Collections
{
    public struct ProducerDelegate<TData> : IProducer<TData>
    {
        private readonly Func<TData?> _producerFunc;
        
        public ProducerDelegate(Func<TData?> producerFunc)
        {
            _producerFunc = producerFunc;
        }
        
        public bool TryPop([MaybeNullWhen(false)] out TData value)
        {
            value = _producerFunc();
            return value != null;
        }
    }
    
    public struct ConcurrentProducerDelegate<TData> : IConcurrentProducer<TData>
    {
        private readonly Func<TData?> _producerFunc;
        
        public ConcurrentProducerDelegate(Func<TData?> producerFunc)
        {
            _producerFunc = producerFunc;
        }
        
        public bool TryPop([MaybeNullWhen(false)] out TData value)
        {
            value = _producerFunc();
            return value != null;
        }
    }
}