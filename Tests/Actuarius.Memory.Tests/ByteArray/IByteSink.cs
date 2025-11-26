using Actuarius.Collections;

namespace Actuarius.Memory.Tests
{
    [TestFixture]
    public class TestIByteSink
    {
        [Test]
        public void TestByteArraySink()
        {
            byte[] bytes = new byte[7];
            var sink = new ByteSinkFromArray();
            sink.Reset(new MultiRefByteArray(bytes), 2);

            Test1_5(sink);

            if (sink.Put(0))
            {
                Assert.Fail();
            }

            Assert.That(new MultiRefByteArray([0, 0, 1, 2, 3, 4, 5]).EqualByContent(new MultiRefByteArray(bytes)), Is.True);

        }

        [Test]
        public void TestRangedByteArraySink()
        {
            byte[] bytes = new byte[5];
            var sink = new ByteSinkFromArray();
            sink.Reset(new MultiRefByteArray(bytes, 1, 3));

            Test1_5(sink);

            Assert.That(new MultiRefByteArray([0, 1, 2, 0, 0]).EqualByContent(new MultiRefByteArray(bytes)), Is.True);
        }

        private void Test1_5(IByteSink sink)
        {
            try
            {
                sink.PutMany(new MultiRefByteArray([]));
                sink.Put(1);
                sink.Put(2);
                sink.PutMany(new MultiRefByteArray([3, 4, 5]));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
        }
    }
}