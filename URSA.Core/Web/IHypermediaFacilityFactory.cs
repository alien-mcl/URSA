namespace URSA.Web
{
    /// <summary>Provides a description of the <see cref="IHypermediaFacility" /> factory.</summary>
    public interface IHypermediaFacilityFactory
    {
        /// <summary>Creates a <see cref="IHypermediaFacility" /> for a given <paramref name="controller" />.</summary>
        /// <param name="controller">Controller for which to create a <see cref="IHypermediaFacility" />.</param>
        /// <returns>New instance of the <see cref="IHypermediaFacility" />.</returns>
        IHypermediaFacility CreateFor(IController controller);
    }
}