namespace Actuarius.Collections.PerfTest.Stream
{
    public struct XorPayload<TPayload>
        where TPayload : struct, IPayload
    {
        public long XorKey;
        public TPayload Payload;
        
        public XorPayload(long xorKey, TPayload payload)
        {
            XorKey = xorKey;
            Payload = payload;
        }
        
        public readonly void ApplyXor(ref long value) 
        {
            value ^= XorKey;
        }
    }
}