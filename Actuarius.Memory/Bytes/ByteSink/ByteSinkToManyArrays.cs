using System;
using Actuarius.Collections;

namespace Actuarius.Memory
{
    public struct ByteSinkToManyArrays<TArray> : IByteSink
        where TArray : IByteArray
    {
        private readonly IProducer<TArray> _emptyArraysProducer;
        private readonly IConsumer<TArray> _filledArraysConsumer;
        
        private TArray? _currentArray;
        private int _currentArraySize;
        private int _currentArrayPosition;
        
        public ByteSinkToManyArrays(IProducer<TArray> emptyArraysProducer, IConsumer<TArray> filledArraysConsumer)
        {
            _emptyArraysProducer = emptyArraysProducer;
            _filledArraysConsumer = filledArraysConsumer;
            _currentArray = default;
            _currentArraySize = 0;
            _currentArrayPosition = 0;
        }
        
        private bool SetupNextBlock()
        {
            if (_currentArray != null)
            {
                return false;
            }

            if (!_emptyArraysProducer.TryPop(out _currentArray))
            {
                return false;
            }

            _currentArraySize = _currentArray.Count;
            _currentArrayPosition = 0;

            return true;
        }

        private bool PublishCurrentBlock()
        {
            if (_currentArray != null && _currentArrayPosition == _currentArraySize)
            {
                var block = _currentArray;
                _currentArray = default;
                return _filledArraysConsumer.Put(block);
            }
            return false;
        }

        public void Finish()
        {
            PublishCurrentBlock();
        }
        
        public bool Put(byte value)
        { 
            if (_currentArray == null)
            {
                if (!SetupNextBlock())
                {
                    return false;
                }
            }

            if (_currentArrayPosition < _currentArraySize)
            {
                _currentArray![_currentArrayPosition++] = value;
                if (_currentArrayPosition == _currentArraySize)
                {
                    return PublishCurrentBlock();
                }
                return true;
            }
            return false;
        }

        public bool PutMany<TBytes>(TBytes bytes)
            where TBytes : IReadOnlyBytes
        {
            int count = bytes.Count;
            int pos = 0;
            while (pos < count)
            {
                if (_currentArray == null)
                {
                    if (!SetupNextBlock())
                    {
                        return false;
                    }
                }
                
                int copyCount = Math.Min(_currentArraySize - _currentArrayPosition, count - pos);
                if (bytes.CopyTo(_currentArray!.Array, _currentArrayPosition, pos, copyCount))
                {
                    pos += copyCount;
                    _currentArrayPosition += copyCount;
                    if (_currentArrayPosition == _currentArraySize)
                    {
                        if (!PublishCurrentBlock())
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}