namespace Actuarius.Memory
{
    /// <summary>
    /// Объект у которого есть много владельцев
    /// </summary>
    public interface IMultiRefResource : IReleasableResource
    {
        /// <summary>
        /// TRUE если объект имеет хотя бы одного владельца
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Увеличивает число владельцев на 1
        /// </summary>
        void AddRef();
    }
    
    public static class IMultiRefResource_Ext
    {
        public static T Acquire<T>(this T element)
            where T : class, IMultiRefResource
        {
            element.AddRef();
            return element;
        }
    }
}