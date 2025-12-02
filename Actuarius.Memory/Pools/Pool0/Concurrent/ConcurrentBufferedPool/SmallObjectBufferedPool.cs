using System;
using Actuarius.Collections;
using Actuarius.Memory.ConcurrentBuffered;

namespace Actuarius.Memory
{
    public class SmallObjectBufferedPool<TObject> : BufferedPool<TObject>
        where TObject : class
    {
        public SmallObjectBufferedPool(Func<TObject> ctor)
            : base(100, 10, ctor, () => new TinyConcurrentQueue<Bucket<TObject>>())
        {
        }
    }
}