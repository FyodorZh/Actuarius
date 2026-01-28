using System.Diagnostics.CodeAnalysis;

namespace Actuarius.Collections
{
    public class SystemConcurrentStack<TData> : IConcurrentQueue<TData>
    {
        private readonly System.Collections.Concurrent.ConcurrentStack<TData> _stack = new();
        
        public bool Put(TData value)
        {
            _stack.Push(value);
            return true;
        }

        public bool TryPop([MaybeNullWhen(false)] out TData value)
        {
            return _stack.TryPop(out value);
        }

        public int Count => _stack.Count;
    }
}