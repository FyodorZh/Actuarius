using System;
using System.Threading;

namespace Actuarius.Memory
{
    /// <summary>
    /// Любой объект допускающий владение собой
    /// </summary>
    public interface IReleasableResource
    {
        /// <summary>
        /// Информирует объект об завершении факта владения
        /// </summary>
        void Release();
    }

    public static class IReleasableResource_Ext
    {
        public static ReleasableResourceDisposer<TResource> AsDisposable<TResource>(this TResource resource)
            where TResource : class, IReleasableResource
        {
            return new ReleasableResourceDisposer<TResource>(resource);
        }
        

        public struct ReleasableResourceDisposer<TResource> : IDisposable
            where TResource : class, IReleasableResource
        {
            private TResource? _resource;

            public readonly TResource Resource => _resource ?? throw new NullReferenceException();
            
            public ReleasableResourceDisposer(TResource resource)
            {
                _resource = resource;
            }

            public void Dispose()
            {
                var resource = Interlocked.Exchange(ref _resource, null);
                resource?.Release();
            }
        }
    }
}