namespace Actuarius.Collections.PerfTest.Stream
{
    public interface IPayload
    {
        void ConfuseOptimizer();
        static abstract IPayload ConstructRandomPayload();
    }
}