namespace Actuarius.Collections
{
    /// <summary>
    /// Потребитель данных
    /// </summary>
    public interface IConsumer<in TData>
    {
        /// <summary>
        /// Кладёт элемент в приёмник.
        /// </summary>
        /// <param name="value"></param>
        /// <returns> FALSE если положить элемент не удалось. Например, исчерпана ёмкость коллекции или что угодно другое </returns>
        bool Put(TData value);
    }
}