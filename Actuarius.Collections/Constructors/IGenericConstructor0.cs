namespace Actuarius.Collections
{
    public interface IGenericConstructor<in TRestriction>
    {
        T Construct<T>() where  T : TRestriction;
    }
}