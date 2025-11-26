using Actuarius.Collections;

namespace Actuarius.Memory
{
    /// <summary>
    /// Абстрактное тредобезопасное хранилище последовательности байт.
    /// </summary>
    public interface IReadOnlyBytes : ICountable
    {
        /// <summary>
        /// False эквивалентно нулевому массиву
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Копирует данные в приёмник.
        /// </summary>
        /// <param name="dst"> Куда скопировать </param>
        /// <param name="dstOffset"> Начиная с какой позиции </param>
        /// <param name="srcOffset"> Номер первого копируемого элемента в источнике</param>
        /// <param name="count"> Количество байт для копирования</param>
        /// <returns> В случае неуспеха возвращает false. Данные в приёмнике остаются в неопределённом состоянии</returns>
        bool CopyTo(byte[] dst, int dstOffset, int srcOffset, int count);
    }

    public static class IReadOnlyBytes_Ext
    {
        public static byte[]? ToArray(this IReadOnlyBytes self, IPool<byte[], int>? pool = null)
        {
            if (!self.IsValid)
            {
                return null;
            }

            int count = self.Count;
            byte[] bytes = pool?.Acquire(count) ?? new byte[count];
            self.CopyTo(bytes, 0, 0, count);
            return bytes;
        }
    }
}