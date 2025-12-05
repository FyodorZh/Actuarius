namespace Actuarius.Memory
{
    public interface IGenericPool<in TResourceRestriction, in TParam>
    {
        IPool<TResource, TParam> ShowTypedPool<TResource>();
        (TResource resource, IPoolSink<TResource> poolSink) Acquire<TResource>(TParam param) where TResource : TResourceRestriction;
    }
    
    public interface IGenericConcurrentPool<in TResourceRestriction, in TParam> : IGenericPool<TResourceRestriction, TParam>
    {
        new IConcurrentPool<TResource, TParam> ShowTypedPool<TResource>();
    }
}