namespace Actuarius.Collections
{
    public interface IArray<TData> : IReadOnlyArray<TData>
    {
        new TData this[int id] { get; set; }
    }
}