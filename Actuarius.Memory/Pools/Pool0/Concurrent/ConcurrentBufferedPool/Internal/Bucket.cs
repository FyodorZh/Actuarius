using System;
using System.Diagnostics.CodeAnalysis;

namespace Actuarius.Memory.ConcurrentBuffered
{
    public class Bucket<TObject>
    {
        private readonly Func<TObject> mCtor;
        private readonly TObject?[] mPool;
        private readonly int mCapacity;

        private int mRealObjectsNumber;
        private int mVirtualObjectsNumber;

        public Bucket(int capacity, Func<TObject> ctor)
        {
            mCtor = ctor;
            mPool = new TObject[capacity];
            mCapacity = capacity;
            mRealObjectsNumber = 0;
            mVirtualObjectsNumber = 0;
        }

        public void LazyFill()
        {
            mVirtualObjectsNumber = mCapacity - mRealObjectsNumber;
        }

        public bool TryPop([MaybeNullWhen(false)] out TObject value)
        {
            if (mRealObjectsNumber > 0)
            {
                int id = --mRealObjectsNumber;
                value = mPool[id]!;
                mPool[id] = default;
                return true;
            }

            if (mVirtualObjectsNumber > 0)
            {
                mVirtualObjectsNumber -= 1;
                value = mCtor.Invoke();
                return true;
            }

            value = default;
            return false;
        }

        public bool Put(TObject value)
        {
            if (mRealObjectsNumber == mCapacity)
            {
                return false;
            }

            mPool[mRealObjectsNumber++] = value;
            if (mRealObjectsNumber + mVirtualObjectsNumber > mCapacity)
            {
                mVirtualObjectsNumber -= 1;
            }

            return true;
        }
    }
}