namespace Actuarius.Memory
{
    public class MultiRefByteArraySpan : MultiRefByteArray
    {
        private IMultiRefResourceOwner<IByteArray>? _source;
        
        public MultiRefByteArraySpan(IMultiRefResourceOwner<IByteArray> source, int offset, int count) 
            : this(source.ExposeResourceUnsafe(out  _), offset, count)
        {
            _source = source.Acquire();
        }
        
        private MultiRefByteArraySpan(IByteArray source, int offset, int count) 
            : base(source.Array, source.Offset + offset, count)
        {
        }

        protected override void OnReleased()
        {
            base.OnReleased();
            _source?.Release();
            _source = null!;
        }
    }
}