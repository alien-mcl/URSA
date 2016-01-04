using System.Threading.Tasks;

namespace URSA.Web
{
    /// <summary>Provides asynchronous write operations of an entity.</summary>
    /// <typeparam name="T">Type of entities.</typeparam>
    /// <typeparam name="I">Type of the identifier of an entity.</typeparam>
    public interface IAsyncWriteController<T, I> : IAsyncReadController<T, I> where T : IControlledEntity<I>
    {
        /// <summary>Creates a new instance.</summary>
        /// <param name="instance">Instance to be created.</param>
        /// <returns>Identifier of the newly create entity.</returns>
        /// <exception cref="URSA.Web.AlreadyExistsException">Thrown when a given instance already exists and cannot be created.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when a given <paramref name="instance" /> provided is out of range.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when a given <paramref name="instance" /> is not provided.</exception>
        /// <exception cref="System.ArgumentException">Thrown when a given <paramref name="instance" /> provided is invalid.</exception>
        Task<I> CreateAsync(T instance);

        /// <summary>Updates an existing instance.</summary>
        /// <param name="id">Identifier of the entity to be updated.</param>
        /// <param name="instance">Instance data.</param>
        /// <returns>Asynchronous task awaiting the operation.</returns>
        /// <exception cref="URSA.Web.NotFoundException">Thrown when a given instance does not exist.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when any of the given parameters provided is out of range.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when any of the given parameters is not provided.</exception>
        /// <exception cref="System.ArgumentException">Thrown when any of the given parameters provided is invalid.</exception>
        Task UpdateAsync(I id, T instance);

        /// <summary>Deletes an entity.</summary>
        /// <param name="id">Identifier of the entity to be deleted.</param>
        /// <returns>Asynchronous task awaiting the operation.</returns>
        /// <exception cref="URSA.Web.NotFoundException">Thrown when a given instance does not exist.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when any of the given parameters provided is out of range.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when any of the given parameters is not provided.</exception>
        /// <exception cref="System.ArgumentException">Thrown when any of the given parameters provided is invalid.</exception>
        Task DeleteAsync(I id);
    }
}