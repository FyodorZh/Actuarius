using System;
namespace Actuarius.Collections.PerfTest.Stream
{
    public class XorTestCase<T> : IXorTestCase<T>
        where T : struct, IPayload
    {
        private readonly StreamCommand<XorPayload<T>>[] _commands;

        public XorTestCase(int size)
        {
            Random rnd = new Random();
            
            _commands = new StreamCommand<XorPayload<T>>[size];
            for (int i = 0; i < size; ++i)
            {
                XorPayload<T> xorPayload = new XorPayload<T>(rnd.NextInt64(), (T)T.ConstructRandomPayload());

                _commands[i] = new StreamCommand<XorPayload<T>>(
                    rnd.Next() % 2 == 0 ? StreamCommandType.Add : StreamCommandType.Remove,
                    xorPayload);
            }
        }
        
        public (long added, long extracted) Run(IUnorderedCollection<XorPayload<T>> stream)
        {
            long added = 0;
            long extracted = 0;

            foreach (var command in _commands)
            {
                switch (command.Type)
                {
                    case StreamCommandType.Add:
                        if (stream.Put(command.Payload))
                        {
                            command.Payload.ApplyXor(ref added);
                        }
                        break;
                    case StreamCommandType.Remove:
                        if (stream.TryPop(out var payload))
                        {
                            payload.ApplyXor(ref extracted);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return (added, extracted);
        }
    }
}