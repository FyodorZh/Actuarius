using System;
using System.Collections.Generic;

namespace Actuarius.Concurrent
{
    /// <summary>
    /// Provides thread-safe, serialized invocation of an action, ensuring that only one 
    /// instance of the action executes at a time even when called concurrently.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When multiple threads call <see cref="ScheduleOneInvokation"/> simultaneously, the provided action
    /// is executed exactly once per concurrent call group, with all pending invocations
    /// completed before any new calls can begin execution. This ensures serialized execution
    /// while allowing concurrent entry.
    /// </para>
    /// <para>
    /// If the action throws exceptions during concurrent execution, all exceptions are
    /// collected and re-thrown as an <see cref="AggregateException"/> once all pending
    /// executions complete.
    /// </para>
    /// <para>
    /// This class is thread-safe and designed for high-contention scenarios where
    /// serialized execution is required but blocking callers is undesirable.
    /// </para>
    /// </remarks>
    public class ConcurrentSerializedExecutor
    {
        private readonly Action _action;
        private int _countToExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentSerializedExecutor"/> class.
        /// </summary>
        /// <param name="action">
        /// The action to execute. This action may be invoked multiple times during
        /// concurrent calls to <see cref="ScheduleOneInvokation"/> but never concurrently with itself.
        /// The action should  be thread-safe as it may be executed by different threads.
        /// </param>
        public ConcurrentSerializedExecutor(Action action)
        {
            _action = action;
        }

        /// <summary>
        /// Executes the action with thread-safe serialization.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When called concurrently by multiple threads, the first thread to enter begins
        /// executing the action. Subsequent concurrent calls are queued and processed
        /// sequentially until all pending invocations complete. During this time, new
        /// callers will schedule its invocation without blocking.
        /// </para>
        /// <para>
        /// If exceptions occur during execution, they are collected and thrown as an
        /// <see cref="AggregateException"/> after all pending actions in the current
        /// batch complete.
        /// </para>
        /// <para>
        /// This method uses lock-free interlocked operations for minimal contention.
        /// </para>
        /// </remarks>
        /// <exception cref="AggregateException">
        /// Thrown when one or more exceptions occur during action execution.
        /// Contains all exceptions thrown by the action during the current invocation batch.
        /// </exception>
        public void ScheduleOneInvokation()
        {
            if (System.Threading.Interlocked.Increment(ref _countToExecute) == 1)
            {
                List<Exception>? exceptions = null;
                while (true)
                {
                    try
                    {
                        _action();
                    }
                    catch (Exception ex)
                    {
                        exceptions ??= new List<Exception>();
                        exceptions.Add(ex);
                    }

                    if (System.Threading.Interlocked.Decrement(ref _countToExecute) == 0)
                    {
                        break;
                    }
                }

                if (exceptions != null)
                {
                    throw new AggregateException(exceptions);
                }
            }
        }
    }
}