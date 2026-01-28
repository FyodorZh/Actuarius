using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Actuarius.Collections.PerfTest.Stream
{
    public class XorTestExecutor<T>
        where T : struct, IPayload
    {
        private readonly IXorTestCase<T>[] _tests;
        
        public XorTestExecutor(IEnumerable<IXorTestCase<T>> tests)
        {
            _tests = tests.ToArray();
        }

        public async Task<(bool isOK, TimeSpan time)> Run(IConcurrentUnorderedCollection<XorPayload<T>> stream)
        {
            Thread[] threads = new Thread[_tests.Length];
            Task[] tasks = new Task[_tests.Length];
            Stopwatch[] stopwatches = new Stopwatch[_tests.Length];
            (long added, long removed)[] results = new (long added, long removed)[_tests.Length];

            SemaphoreSlim semaphore = new SemaphoreSlim(0, _tests.Length);
            
            for (int i = 0; i < _tests.Length; ++i)
            {
                int threadId = i;
                TaskCompletionSource tcs = new();
                tasks[i] = tcs.Task;
                threads[i] = new Thread(() =>
                {
                    semaphore.Wait();
                    
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    
                    results[threadId] = _tests[threadId].Run(stream);
                    
                    sw.Stop();
                    stopwatches[threadId] = sw;
                    
                    tcs.SetResult();
                });
                threads[i].Start();
            }

            semaphore.Release(_tests.Length);
            await Task.WhenAll(tasks);

            
            long added = 0;
            long removed = 0;
            while (stream.TryPop(out var value))
            {
                value.ApplyXor(ref removed);
            }

            TimeSpan totalTime = TimeSpan.Zero;
            for (int i = 0; i < _tests.Length; ++i)
            {
                totalTime += stopwatches[i].Elapsed;
                added ^= results[i].added;
                removed ^= results[i].removed;
            }

            return (added == removed, totalTime);
        }
    }
}