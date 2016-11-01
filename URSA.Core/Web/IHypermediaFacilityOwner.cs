namespace URSA.Web
{
    /// <summary>Describes a hypermedia facility owner.</summary>
    public interface IHypermediaFacilityOwner
    {
        /// <summary>Gets or sets a owner's hypermedia facility.</summary>
        IHypermediaFacility HypermediaFacility { get; set; }
    }
}