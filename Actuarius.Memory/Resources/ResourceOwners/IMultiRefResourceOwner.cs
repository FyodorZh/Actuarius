namespace Actuarius.Memory
{
    public interface IMultiRefResourceOwner<TResource> : IMultiRefResource
        where TResource : class
    {
        TResource ExposeResourceUnsafe(out TResource resource);
    }

    public static class IMultiRefResourceOwner_Ext
    {
        public static ReleasableResourceAccessor<TResource> ExposeResourceOnce<TResource>(this IMultiRefResourceOwner<TResource> owner)
            where TResource : class
        {
            owner.ExposeResourceUnsafe(out var resource);
            return new ReleasableResourceAccessor<TResource>(resource, owner.Acquire());
        }
    }
}