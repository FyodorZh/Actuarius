using System;
using Actuarius.Collections;

namespace Actuarius.Memory.ConcurrentBuffered
{
    public class BucketSource<TObject>
    {
        private readonly int mBucketSize;
        private readonly Func<TObject> mObjectCtor;

        private readonly IConcurrentUnorderedCollection<Bucket<TObject>> mFullBuckets;
        private readonly IConcurrentUnorderedCollection<Bucket<TObject>> mEmptyBuckets;

        public BucketSource(int bucketSize, Func<TObject> objectCtor, Func<IConcurrentUnorderedCollection<Bucket<TObject>>> ctor)
        {
            mBucketSize = bucketSize;
            mObjectCtor = objectCtor;
            mFullBuckets = ctor.Invoke();
            mEmptyBuckets = ctor.Invoke();
        }

        public Bucket<TObject> GetFullBucket()
        {
            if (!mFullBuckets.TryPop(out var bucket))
            {
                bucket = GetEmptyBucket();
                bucket.LazyFill();
            }

            return bucket;
        }

        public Bucket<TObject> GetEmptyBucket()
        {
            if (!mEmptyBuckets.TryPop(out var bucket))
            {
                bucket = new Bucket<TObject>(mBucketSize, mObjectCtor);
            }

            return bucket;
        }

        public bool ReturnFullBucket(Bucket<TObject> bucket)
        {
            return mFullBuckets.Put(bucket);
        }

        public bool ReturnEmptyBucket(Bucket<TObject> bucket)
        {
            return mEmptyBuckets.Put(bucket);
        }
    }
}