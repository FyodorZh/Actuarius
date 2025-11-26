namespace Actuarius.Memory
{
    public class NoPool<TObject> : IConcurrentPool<TObject>
        where TObject : class, new()
    {
        public static readonly NoPool<TObject> Instance = new NoPool<TObject>();
        
        public TObject Acquire()
        {
            return new TObject();
        }
        
        public void Release(TObject? obj)
        {
        }
    }
}