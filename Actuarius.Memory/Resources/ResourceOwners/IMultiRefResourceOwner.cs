namespace Actuarius.Memory
{
    public interface IMultiRefResourceOwner<TResource> : IMultiRefResource
        where TResource : class
    {
        ReleasableResourceAccessor<TResource> GetAccessor();
    }
}