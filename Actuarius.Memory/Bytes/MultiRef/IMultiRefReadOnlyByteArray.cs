namespace Actuarius.Memory
{
    public interface IMultiRefReadOnlyByteArray : IMultiRefReadOnlyBytes, IMultiRefResourceOwner<IReadOnlyByteArray>, IReadOnlyByteArray
    {
    }
}