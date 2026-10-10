using System;

namespace Actuarius.Memory
{
    /// <summary>
    /// Debug wrapper on top of IMultiRefByteArray that can guarantee that inner object is used correctly
    /// </summary>
    public sealed class MultiRefByteArrayGuard : MultiRefResource, IMultiRefByteArray
    {
        private volatile IMultiRefByteArray? _core;
        
        public MultiRefByteArrayGuard(IMultiRefByteArray core)
            : base(false)
        {
            _core = core;
        }

        protected override void OnReleased()
        {
            var core = _core;
            if (core != null)
            {
                _core = null;
                core.Release();
            }
            else
            {
                throw new ObjectDisposedException("MultiRefByteArray");
            }
        }

        public int Count => _core?.Count ?? throw new ObjectDisposedException("MultiRefByteArray");

        public bool IsValid => _core?.IsValid ?? false;

        public bool CopyTo(byte[] dst, int dstOffset, int srcOffset, int count)
        {
            return _core?.CopyTo(dst, dstOffset, srcOffset, count) ?? throw new ObjectDisposedException("MultiRefByteArray");
        }

        public byte this[int id]
        {
            get => _core?[id] ?? throw new ObjectDisposedException("MultiRefByteArray");
            set
            {
                var core = _core;
                if (core == null)
                {
                    throw new ObjectDisposedException("MultiRefByteArray");
                }
                core[id] = value;
            }
        }

        public byte[] ReadOnlyArray => _core?.ReadOnlyArray ?? throw new ObjectDisposedException("MultiRefByteArray");

        public int Offset => _core?.Offset ?? throw new ObjectDisposedException("MultiRefByteArray");

        public byte[] Array => _core?.Array ?? throw new ObjectDisposedException("MultiRefByteArray");
    }
}