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
            var sink = new ByteSinkToArray(bytes, 2);

            Test1_5(ref sink);

            if (sink.Put(0))
            {
                Assert.Fail();
            }

            Assert.That(bytes, Is.EquivalentTo([0, 0, 1, 2, 3, 4, 5]));

        }

        [Test]
        public void TestRangedByteArraySink()
        {
            byte[] bytes = new byte[5];
            var sink = new ByteSinkToArray(bytes, 1, 3);

            Test1_5(ref sink);

            Assert.That(bytes, Is.EquivalentTo([0, 1, 2, 0, 0]));
        }

        private void Test1_5<TByteSink>(ref TByteSink sink)
            where TByteSink : IByteSink
        {
            try
            {
                sink.PutMany(new StaticByteArray([]));
                sink.Put(1);
                sink.Put(2);
                sink.PutMany(new StaticByteArray([3, 4, 5]));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
        }
    }
}