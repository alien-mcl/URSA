namespace URSA.Web
{
    /// <summary>Represents an entity controlled by the <see cref="IReadController{T, I}" />.</summary>
    /// <typeparam name="T">Type of the entity's identifier.</typeparam>
    public interface IControlledEntity<T>
    {
        /// <summary>Gets or sets the primary key.</summary>
        T Id { get; set; }
    }
}