namespace Actuarius.Memory
{
    public interface IMultiRefResourceOwner<TResource> : IMultiRefResource
        where TResource : class
    {
        TResource ShowResourceUnsafe(out TResource resource);
    }

    public static class IMultiRefResourceOwner_Ext
    {
        public static ReleasableResourceAccessor<TResource> ExposeAccessorOnce<TResource>(this IMultiRefResourceOwner<TResource> owner)
            where TResource : class
        {
            owner.ShowResourceUnsafe(out var resource);
            return new ReleasableResourceAccessor<TResource>(resource, owner.Acquire());
        }
    }
}