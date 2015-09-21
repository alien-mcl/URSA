namespace URSA.Web
{
    /// <summary>Provides a contract of a read controller.</summary>
    /// <typeparam name="T">Type of entities retrieved.</typeparam>
    /// <typeparam name="I">Type of the identifier of entity.</typeparam>
    public interface IReadController<T, I> : IController<T> where T : IControlledEntity<I>
    {
        /// <summary>Gets the given entity.</summary>
        /// <param name="id">Identifier of the entity to retrieve.</param>
        /// <returns>Instance of the <typeparamref name="T" /> if matching <paramref name="id" />; otherwise <b>null</b>.</returns>
        T Get(I id);
    }
}