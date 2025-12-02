using System;
using Actuarius.Collections;
using Actuarius.Memory.ConcurrentBuffered;

namespace Actuarius.Memory
{
    public class LargeObjectBufferedPool<TObject> : BufferedPool<TObject>
        where TObject : class
    {
        public LargeObjectBufferedPool(Func<TObject> ctor)
            : base(20, 3, ctor, () => new TinyConcurrentQueue<Bucket<TObject>>())
        {
        }
    }
}