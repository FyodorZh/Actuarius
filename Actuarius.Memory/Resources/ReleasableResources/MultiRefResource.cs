//#define TRACE_HISTORY

#if TRACE_HISTORY
    #define TRACE_HISTORY_ALL
    #define TRACE_DESTRUCTOR
    #define TRACE_FILTER_BY_TYPE
#elif DEBUG
    //#define TRACE_DESTRUCTOR
#endif

using System.Diagnostics;
using Actuarius.Memory.Internal;


namespace Actuarius.Memory
{
    public abstract class MultiRefResource : IMultiRefResource
    {
        protected enum ResourceUsageErrorType
        {
            AddRefOfReleasedObject,
            ReleaseOfReleasedObject,
            WrongReviveUsage,
            Leak,
            NoUsageAssertionFail,
        }

        private readonly RefCounter _refCounter = new RefCounter();

        protected virtual bool TraceEnabled => true;

#if TRACE_HISTORY
        private readonly ActionHistoryTracer _tracer = new ActionHistoryTracer();

        private void Trace(string name)
        {
            #if TRACE_FILTER_BY_TYPE
            if (TraceEnabled)
            #endif
            {
                _tracer.RecordEvent(name);
            }
        }

        private ActionHistoryTracer? GetTracer() => _tracer;
#else
        private ActionHistoryTracer? GetTracer() => null;
#endif

        protected MultiRefResource(bool noInit)
        {
            if (!noInit)
            {
#if TRACE_HISTORY
                Trace("Ctor");
#endif
                _refCounter.Init();
            }
        }

#if TRACE_DESTRUCTOR
        ~MultiRefResource()
        {
            if (_refCounter.IsValid)
            {
                OnRefCountError(ResourceUsageErrorType.Leak, null);
            }
        }
#endif

        protected abstract void OnReleased();

        protected virtual void OnRefCountError(ResourceUsageErrorType error, ActionHistoryTracer? tracer)
        {
            string text = $"Invalid MultiRef object of type {GetType()} usage. Error = {error}";
//             Log.w(text);
// #if TRACE_HISTORY
//             var history = _tracer.Export();
//             foreach (var stack in history)
//             {
//                 Log.w(stack.Action + "\n" + stack.Stack.ToString());
//             }
// #endif
            Debug.Assert(false, text);
        }

        public bool IsAlive => _refCounter.IsValid;

        public void AddRef()
        {
#if TRACE_HISTORY_ALL
            Trace("AddRef");
#endif
            if (_refCounter.AddRef() == 0)
            {
                OnRefCountError(ResourceUsageErrorType.AddRefOfReleasedObject, GetTracer());
            }
        }

        public void Release()
        {
            int cnt = _refCounter.Release();

#if TRACE_HISTORY
        #if TRACE_HISTORY_ALL
            Trace("Release");
        #else
            if (cnt == 0)
            {
                Trace("Release");
            }
        #endif
#endif

            if (cnt == 0)
            {
                OnReleased();
            }
            else if (cnt < 0)
            {
                OnRefCountError(ResourceUsageErrorType.ReleaseOfReleasedObject, GetTracer());
            }
        }

        protected bool Revive()
        {
#if TRACE_HISTORY
            _tracer.Clear();
            Trace("Revive");
#endif
            if (!_refCounter.Revive())
            {
                OnRefCountError(ResourceUsageErrorType.WrongReviveUsage, GetTracer());
                return false;
            }

            return true;
        }

        protected void AssertNoUsage()
        {
            if (_refCounter.IsValid)
            {
                OnRefCountError(ResourceUsageErrorType.NoUsageAssertionFail, GetTracer());
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name}: {_refCounter}";
        }
    }
}