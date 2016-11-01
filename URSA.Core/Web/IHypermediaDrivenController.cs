namespace URSA.Web
{
    /// <summary>Describes a hypermedia driven controller.</summary>
    /// <typeparam name="TController">Type of the controller in force.</typeparam>
    /// <typeparam name="TEntity">Type of the entity being controlled.</typeparam>
    public interface IHypermediaDrivenController<TController, TEntity> : IController<TEntity>, IHypermediaFacilityOwner
        where TController : IController<TEntity>
    {
    }
}