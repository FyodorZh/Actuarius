using System.Diagnostics.CodeAnalysis;

namespace Actuarius.Collections
{
    /// <summary>
    /// Источник данных
    /// </summary>
    public interface IProducer<TData>
    {
        /// <summary>
        /// Получает очередной элемент из продюсера
        /// </summary>
        /// <param name="value"> Возвращаемый элемент </param>
        /// <returns> FALSE если очередной элемент получить не удалось </returns>
        bool TryPop([MaybeNullWhen(false)] out TData value);
    }
}