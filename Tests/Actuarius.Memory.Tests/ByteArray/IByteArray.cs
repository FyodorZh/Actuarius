using Actuarius.Collections;

namespace Actuarius.Memory.Tests
{
    [TestFixture]
    public class TestIByteArray
    {
        [Test]
        public void TestByteArraySegment()
        {
            Test0_5(new MultiRefByteArray([0, 1, 2 ,3, 4, 5]), [0, 1, 2, 3, 4, 5]);
        }

        [Test]
        public void TestCollectableAbstractByteArraySegment()
        {
            var segment = new MultiRefByteArraySpan(new MultiRefByteArray([0, 1, 2, 3, 4, 5, 6, 7, 8]), 2, 5);
            Test0_5(segment, [2, 3, 4, 5, 6]);
            segment.Release();
        }

        // [Test]
        // public void TestCollectableMultiSegmentByteArray()
        // {
        //     var segment = ConcurrentPools.Acquire<CollectableMultiSegmentByteArray>().Init(new IMultiRefByteArray[] {
        //             new MultiRefByteArray(new byte[]{1, 2 ,3 }),
        //             new MultiRefByteArray(new byte[]{ }),
        //             new MultiRefByteArray(new byte[]{4}),
        //             new MultiRefByteArray(new byte[]{5, 6 })
        //     });
        //
        //     Test0_5(segment, new byte[]{1, 2, 3, 4, 5, 6});
        //
        //     segment.Release();
        // }

        // [Test]
        // public void TestMemoryBufferHolder()
        // {
        //     MemoryRental.Shared.CollectablePool.Acquire<UnionDa>()
        //     using (var bufferAccessor = ConcurrentUsageMemoryBufferPool.Instance.Allocate().ExposeAccessorOnce())
        //     {
        //         bufferAccessor.Buffer.PushInt32(1234512345);
        //         bufferAccessor.Buffer.PushArray(new MultiRefByteArray([1, 2]));
        //
        //         using (var sub = ConcurrentUsageMemoryBufferPool.Instance.Allocate().ExposeAccessorOnce())
        //         {
        //             sub.Buffer.PushAbstractArray(new MultiRefByteArray([7, 8]));
        //             bufferAccessor.Buffer.PushBuffer(sub.Acquire());
        //         }
        //
        //         var holder = bufferAccessor.Acquire();
        //         byte[] bytes = holder.ToRawArray();
        //
        //         Test0_5(holder, bytes);
        //         holder.Release();
        //     }
        // }

        private void Test0_5(IReadOnlyBytes array, byte[] bytes)
        {
            Assert.That(array.IsValid, Is.True);
            Assert.That(array.Count, Is.EqualTo(bytes.Length));
            Assert.That(array.ToArray(), Is.EquivalentTo(bytes));

            int cnt = 0;

            System.Random rnd = new Random(123);
            for (int i = 0; i < 10000; ++i)
            {
                byte[] dst1 = new byte[bytes.Length * 2];
                byte[] dst2 = new byte[bytes.Length * 2];

                int dstOffset = rnd.Next(bytes.Length + 3) - 3;
                int srcOffset = rnd.Next(bytes.Length + 3) - 3;
                int count = rnd.Next(bytes.Length + 3) - 3;


                bool res1;
                try
                {
                    Buffer.BlockCopy(bytes, srcOffset, dst1, dstOffset, count);
                    res1 = true;
                }
                catch
                {
                    res1 = false;
                }

                bool res2 = array.CopyTo(dst2, dstOffset, srcOffset, count);

                if (res1 != res2)
                {
                    res2 = array.CopyTo(dst2, dstOffset, srcOffset, count);
                }

                Assert.That(res1, Is.EqualTo(res2));
                if (res1)
                {
                    ++cnt;
                    Assert.That(new MultiRefByteArray(dst1).EqualByContent(new MultiRefByteArray(dst2)));
                }
            }

            Console.WriteLine("Count = " + cnt);
        }
    }
}