namespace Actuarius.Memory
{
    /// <summary>
    /// Любой объект допускающий владение собой
    /// </summary>
    public interface IReleasableResource
    {
        /// <summary>
        /// Информирует объект об завершении факта владения
        /// </summary>
        void Release();
    }
}