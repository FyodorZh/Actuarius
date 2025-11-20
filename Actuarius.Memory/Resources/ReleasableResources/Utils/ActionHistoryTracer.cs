using System;
using System.Collections.Generic;
using System.Diagnostics;
using Actuarius.Collections;

namespace Actuarius.Memory.Internal
{
    public class ActionHistoryTracer
    {
        private volatile IConcurrentQueue<Record> _history = new TinyConcurrentQueue<Record>();

        public void RecordEvent(string name, int skipFrames = 1)
        {
            var r = new Record(name, new StackTrace(skipFrames + 1, true));
            _history.Put(r);
        }

        public void Clear()
        {
            _history = new TinyConcurrentQueue<Record>();
        }

        public List<Record> Export()
        {
            List<Record> list = new List<Record>();

            Record r;
            while (_history.TryPop(out r))
            {
                list.Add(r);
            }

            return list;
        }
        
        public readonly struct Record
        {
            public readonly string Action;
            public readonly StackTrace Stack;

            public Record(string action, StackTrace stack)
            {
                Action = action;
                Stack = stack;
            }

            public override string ToString()
            {
                string text = Action + ": ";
                for (int i = 0; i < Math.Min(2, Stack.FrameCount); ++i)
                {
                    text += Stack.GetFrame(i) + "  ||  ";
                }

                return text;
            }
        }
    }
}