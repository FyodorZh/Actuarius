using Actuarius.Memory.Internal;

namespace Actuarius.Memory.Tests
{
    internal sealed class FakeRefCounter : IRefCounterView
    {
        public FakeRefCounter(int refCount = 0)
        {
            RefCount = refCount;
        }

        public int RefCount { get; set; }

        public event Action<int>? Updated;

        public event Action<RefCounterErrorEvent, int>? Error;

        public void EmitUpdated(int count)
        {
            RefCount = count;
            Updated?.Invoke(count);
        }

        public void EmitError(RefCounterErrorEvent error, int count)
        {
            RefCount = count;
            Error?.Invoke(error, count);
        }
    }
}
