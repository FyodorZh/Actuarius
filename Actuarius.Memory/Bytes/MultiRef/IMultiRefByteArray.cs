namespace Actuarius.Memory
{
    public interface IMultiRefByteArray : IMultiRefReadOnlyByteArray, IMultiRefResourceOwner<IByteArray>, IByteArray
    {
    }
}