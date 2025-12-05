using System.Threading;

namespace Actuarius.Memory
{
    public static class TypeToIntStaticMap
    {
        private static int _nextTypeId = -1;
        // ReSharper disable once UnusedTypeParameter
        private static class TypeMap<TResource>
        {
            // ReSharper disable once StaticMemberInGenericType
            public static readonly int TypeId = Interlocked.Increment(ref _nextTypeId); 
        }
        
        public static int GetTypeId<T>() => TypeMap<T>.TypeId;
    }
}