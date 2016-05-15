namespace URSA.Web
{
    /// <summary>Describes a hypermedia driven asynchronous controller.</summary>
    /// <typeparam name="TController">Type of the controller in force.</typeparam>
    /// <typeparam name="TEntity">Type of the entity being controlled.</typeparam>
    public interface IHypermediaDrivenAsyncController<TController, TEntity> : IAsyncController<TEntity>
        where TController : IAsyncController<TEntity>
    {
    }
}