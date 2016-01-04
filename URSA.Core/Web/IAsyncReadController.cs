using System.Threading.Tasks;

namespace URSA.Web
{
    /// <summary>Provides a contract of an asynchronous read controller.</summary>
    /// <typeparam name="T">Type of entities retrieved.</typeparam>
    /// <typeparam name="I">Type of the identifier of entity.</typeparam>
    public interface IAsyncReadController<T, I> : IAsyncController<T> where T : IControlledEntity<I>
    {
        /// <summary>Gets the given entity.</summary>
        /// <param name="id">Identifier of the entity to retrieve.</param>
        /// <returns>Instance of the <typeparamref name="T" /> if matching <paramref name="id" />; otherwise <b>null</b>.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when any of the given parameters provided is out of range.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when any of the given parameters is not provided.</exception>
        /// <exception cref="System.ArgumentException">Thrown when any of the given parameters provided is invalid.</exception>
        Task<T> GetAsync(I id);
    }
}