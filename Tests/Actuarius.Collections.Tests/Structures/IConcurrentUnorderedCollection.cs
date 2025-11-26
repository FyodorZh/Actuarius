using System.Collections.Concurrent;

namespace Actuarius.Collections.Tests
{
    [TestFixture]
    public class IConcurrentUnorderedCollection
    {
        [Test]
        public void TestTinyConcurrentQueue()
        {
            Test1(new TinyConcurrentQueue<int>(), (int)1e5, 0.5001);
            Test2(new TinyConcurrentQueue<int>(), 10, 100, 5.0);
        }

        [Test]
        public void TestLimitedConcurrentQueue()
        {
            Test1(new LimitedConcurrentQueue<int>(10000), (int)1e5, 0.5001);
            Test2(new LimitedConcurrentQueue<int>(10 * 100), 10, 100, 5.0);
        }

        [Test]
        public void TestQueueBasedConcurrentUnorderedCollection()
        {
            //Test1(new QueueBasedConcurrentUnorderedCollection<int>(10), (int)1e6, 0.5001);
            //Test2(new QueueBasedConcurrentUnorderedCollection<int>(10), 10, 100, 5.0);
        }

        private void Test1(IConcurrentUnorderedCollection<int> collection, int testSize, double percentOfAddition)
        {
            int prevElement = 0;

            ConcurrentDictionary<int, int> table = new ConcurrentDictionary<int, int>();

            Thread[] threads = new Thread[10];
            for (int k = 0; k < threads.Length; ++k)
            {
                threads[k] = new Thread(() =>
                {
                    Random rnd = new Random();

                    for (int i = 0; i < testSize; ++i)
                    {
                        if (rnd.NextDouble() < percentOfAddition)
                        {
                            int next = Interlocked.Increment(ref prevElement);
                            Assert.That(table.TryAdd(next, 0), Is.True, "Failed to add");
                            Assert.That(collection.Put(next), Is.True);
                        }
                        else
                        {
                            if (collection.TryPop(out var toRemove))
                            {
                                Assert.That(table.TryRemove(toRemove, out _), Is.True, "Failed to remove");
                            }
                            else
                            {
                                //if (table.Count != 0)
                                //{
                                //    collection.TryDequeue(out toRemove);
                                //}
                                //Assert.AreEqual(0, table.Count);
                            }
                        }
                    }
                });
                threads[k].Start();
            }

            foreach (var tr in threads)
            {
                tr.Join();
            }

            Console.WriteLine("Final collection size is " + table.Count);
            while (collection.TryPop(out var toRemove2))
            {
                Assert.That(table.TryRemove(toRemove2, out _), Is.True);
            }

            Assert.That(table.Count, Is.Zero);
        }

        private void Test2(IConcurrentUnorderedCollection<int> collection, int numThreads, int workSize, double seconds)
        {
            int inProgress = 0;

            long sum = 0;

            DateTime now = DateTime.UtcNow;

            Thread[] threads = new Thread[numThreads];
            for (int k = 0; k < threads.Length; ++k)
            {
                Interlocked.Increment(ref inProgress);
                threads[k] = new Thread(() =>
                {
                    int val = 0;
                    while ((DateTime.UtcNow - now).TotalSeconds < seconds)
                    {
                        for (int i = 0; i < workSize; ++i)
                        {
                            val += 1;
                            Interlocked.Add(ref sum, val);
                            Assert.That(collection.Put(val), Is.True);
                        }

                        for (int i = 0; i < workSize; ++i)
                        {
                            while (collection.TryPop(out var x))
                            {
                                Interlocked.Add(ref sum, -x);
                            }
                        }
                    }
                    Interlocked.Decrement(ref inProgress);
                });
                threads[k].Start();
            }

            while (inProgress != 0)
            {
                Thread.Sleep(100);
            }
            
            while (collection.TryPop(out var x))
            {
                Interlocked.Add(ref sum, -x);
            }
            
            Assert.That(sum, Is.Zero);
            
            Assert.Pass("Time = " + (DateTime.UtcNow - now).TotalSeconds + "s");
        }
    }
}