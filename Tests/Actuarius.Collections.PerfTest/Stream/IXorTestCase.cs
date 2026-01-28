namespace Actuarius.Collections.PerfTest.Stream
{
    public interface IXorTestCase<T>
        where T : struct, IPayload
    {
        (long added, long extracted) Run(IUnorderedCollection<XorPayload<T>> stream);
    }
}