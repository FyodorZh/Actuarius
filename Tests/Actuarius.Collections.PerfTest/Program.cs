using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Actuarius.Collections.PerfTest.Stream.Concurrent
{
    public static class Program
    {
        public struct Empty : IPayload
        {
            public void ConfuseOptimizer()
            {
            }

            public static IPayload ConstructRandomPayload()
            {
                return new Empty();
            }
        }

        public static async Task Main()
        {
            Func<IConcurrentUnorderedCollection<XorPayload<Empty>>>[] streams =
            [
                () => new SystemConcurrentQueue<XorPayload<Empty>>(),
                () => new SynchronizedByLockConcurrentQueue<XorPayload<Empty>>(new SystemQueue<XorPayload<Empty>>()),
                () => new SynchronizedBySpinLockConcurrentQueue<XorPayload<Empty>>(new SystemQueue<XorPayload<Empty>>()),
                () => new TinyConcurrentQueue<XorPayload<Empty>>(),
                () => new LimitedConcurrentQueue<XorPayload<Empty>>(10000),
                () => new SystemConcurrentStack<XorPayload<Empty>>(),
                () => new SynchronizedByLockConcurrentStack<XorPayload<Empty>>(),
                () => new SynchronizedBySpinLockConcurrentStack<XorPayload<Empty>>(),
                () => new SystemConcurrentUnorderedCollection<XorPayload<Empty>>()
            ];

            int JobSize = 1000 * 1000 * 100;
            int MaxParallelism = 4;
            
            List<XorTestCase<Empty>> tests = new List<XorTestCase<Empty>>();
            for (int i = 0; i < MaxParallelism; ++i)
            {
                tests.Add(new XorTestCase<Empty>(JobSize));
            }
            
            for (int n = 1; n <= MaxParallelism; ++n)
            {
                Console.WriteLine($"\n Processors: {n}");
                foreach (var streamFactory in streams)
                {
                    var stream = streamFactory.Invoke();
                    
                    XorTestExecutor<Empty> executor = new XorTestExecutor<Empty>(tests.Take(n));
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var res = await executor.Run(stream);
                    sw.Stop();
                    Console.WriteLine($"{stream.GetType().Name,-50}: IsOK: {res.isOK}; Time: {sw.Elapsed}; ThreadTime: {res.time / n}");
                }
            }
        }
    }
}