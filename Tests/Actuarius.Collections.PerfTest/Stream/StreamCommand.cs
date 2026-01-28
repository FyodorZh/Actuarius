namespace Actuarius.Collections.PerfTest.Stream
{
    public enum StreamCommandType : byte
    {
        Add,
        Remove
    }
    
    public struct StreamCommand<TPayload>
        where TPayload : struct
    {
        public StreamCommandType Type;

        public TPayload Payload;

        public StreamCommand(StreamCommandType type, TPayload payload)
        {
            Type = type;
            Payload = payload;
        }
    }
}