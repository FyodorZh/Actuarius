namespace Actuarius.Memory
{
    public class NoPool<TResource> : IConcurrentPool<TResource>
        where TResource : class, new()
    {
        public static readonly NoPool<TResource> Instance = new NoPool<TResource>();
        
        public TResource Acquire()
        {
            return new TResource();
        }
        
        public void Release(TResource? obj)
        {
            // DO NOTHING
        }
    }
}