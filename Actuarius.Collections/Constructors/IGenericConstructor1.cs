namespace Actuarius.Collections
{
    public interface IGenericConstructor<in TRestriction, in TParam>
    {
        T Construct<T>(TParam param) where T : TRestriction;
    }
}