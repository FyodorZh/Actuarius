using System;
using Actuarius.Collections;
using Actuarius.Memory.ConcurrentBuffered;

namespace Actuarius.Memory
{
    public class BufferedPool<TObject> : IConcurrentPool<TObject>
        where TObject : class
    {
        private readonly PoolAccessor<TObject> _multiPool;

        public BufferedPool(int bucketSize, int distributionLevel, Func<TObject> acquire)
            : this(bucketSize, distributionLevel, acquire, () => new SystemConcurrentQueue<Bucket<TObject>>())
        {}

        public BufferedPool(int bucketSize, int distributionLevel, Func<TObject> ctor, Func<IConcurrentUnorderedCollection<Bucket<TObject>>> collectionConstructor)
        {
            var bucketSource = new BucketSource<TObject>(bucketSize, ctor, collectionConstructor);
            _multiPool = new PoolAccessor<TObject>(bucketSource, distributionLevel);
        }

        public TObject Acquire()
        {
            TObject obj;

            // Берём локальный бакет в руку
            ConcurrentBuffered.Pool<TObject> localPool = _multiPool.Get();

            bool failedToReturnEmptyBucket;

            try
            {
                obj = localPool.Acquire(out failedToReturnEmptyBucket);
            }
            finally
            {
                _multiPool.Return(localPool);
            }

            if (failedToReturnEmptyBucket)
            {
                //Log.e("Free buckets pool overflow in {0}", GetType());
            }

            return obj;
        }

        public void Release(TObject? obj)
        {
            if (obj == null)
            {
                return;
            }

            // Берём локальный бакет в руку
            ConcurrentBuffered.Pool<TObject> localPool = _multiPool.Get();

            bool failedToReturnFullBucket;
            bool emptyBucketOverflow;
            try
            {
                localPool.Release(obj, out failedToReturnFullBucket, out emptyBucketOverflow);
            }
            finally
            {
                _multiPool.Return(localPool);
            }

            if (failedToReturnFullBucket)
            {
                //Log.e("Full buckets overflow in {0}", GetType());
            }

            if (emptyBucketOverflow)
            {
                //Log.e("Empty bucket overflow in {0}. Wtf!", GetType());
            }
        }
    }
}