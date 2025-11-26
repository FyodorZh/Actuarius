namespace Actuarius.Collections.Tests
{
    [TestFixture]
    public class IUnorderedCollection
    {
        [Test]
        public void TestCycleQueue()
        {
            Test(new CycleQueue<int>(10), (int)1e6, 0.5001, id => ++id);
        }

        [Test]
        public void TestQueueBasedConcurrentUnorderedCollection()
        {
            //Test(new QueueBasedConcurrentUnorderedCollection<int>(10), (int)1e6, 0.5001, id => ++id);
        }

        [Test]
        public void TestTinyConcurrentQueue()
        {
            Test(new TinyConcurrentQueue<int>(), (int)1e6, 0.5001, id => ++id);
        }

        [Test]
        public void TestLimitedConcurrentQueue()
        {
            Test(new LimitedConcurrentQueue<int>(10000), (int)1e6, 0.5001, id => ++id);
        }

        [Test]
        public void TestPriorityQueue()
        {
            Test(new PriorityQueue<int, int>(), (int)1e6, 0.5001, kv => new KeyValuePair<int, int>((kv.Key * 31 + 371) % 10001, kv.Value + 1));
        }

        private void Test<TElement>(IUnorderedCollection<TElement> collection, int testSize, double percentOfAddition, Func<TElement, TElement> nextElement)
            where TElement : struct
        {
            TElement prevElement = default(TElement);

            Random rnd = new Random();

            HashSet<TElement> table = new HashSet<TElement>();

            TElement toRemove;
            for (int i = 0; i < testSize; ++i)
            {
                if (rnd.NextDouble() < percentOfAddition)
                {
                    TElement next = nextElement(prevElement);
                    prevElement = next;
                    Assert.That(collection.Put(next), Is.True);
                    Assert.That(table.Add(next), Is.True);
                }
                else
                {
                    if (collection.TryPop(out toRemove))
                    {
                        Assert.That(table.Remove(toRemove), Is.True);
                    }
                    else
                    {
                        Assert.That(table.Count, Is.Zero);
                    }
                }
            }

            Console.WriteLine("Final collection size is " + table.Count);
            while (collection.TryPop(out toRemove))
            {
                Assert.That(table.Remove(toRemove), Is.True);
            }

            Assert.That(table.Count, Is.Zero);
        }
    }
}