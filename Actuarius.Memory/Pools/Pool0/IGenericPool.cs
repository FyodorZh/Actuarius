namespace Actuarius.Memory
{
    public interface IGenericPool
    {
        (TResource resource, IPoolSink<TResource> poolSink) Acquire<TResource>() where TResource : class, new();
    }
    
    public interface IGenericConcurrentPool : IGenericPool
    {
    }
}