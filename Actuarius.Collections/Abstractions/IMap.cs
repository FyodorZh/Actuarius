using System.Diagnostics.CodeAnalysis;

namespace Actuarius.Collections
{
    public interface IMap<in TKey, TData>
    {
        /// <summary>
        /// TRUE - элемент был добавлен в контейнер
        /// FALSE - элемент не был добавлен в контейнер по одной из причин
        ///  - Элемент с заданным ключом уже присутствует в контейнере
        ///  - Ёмкость контейнера исчерпана
        /// </summary>
        /// <param name="key"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        bool Add(TKey key, TData element);
        bool Remove(TKey key);
        bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TData element);
    }

    public interface IConcurrentMap<in TKey, TData> : IMap<TKey, TData>
    {
        /// <summary>
        /// Атомарно добавляет новый элемент или перезаписывает существующмй.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newElement"></param>
        /// <param name="oldElement"></param>
        /// <returns> FALSE если добавить элемент не удалось</returns>
        bool AddOrReplace(TKey key, TData newElement, [MaybeNullWhen(false)] out TData oldElement);
        
        /// <summary>
        /// Если элемент с заданным ключом не существует, то он будет добавлен.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newElement"></param>
        /// <param name="resultElement"> </param>
        /// <returns></returns>
        bool AddOrGet(TKey key, TData newElement, [MaybeNullWhen(false)] out TData resultElement);
    }
}