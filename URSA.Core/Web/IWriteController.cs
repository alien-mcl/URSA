namespace URSA.Web
{
    /// <summary>Provides write operations of an entity.</summary>
    /// <typeparam name="T">Type of entities.</typeparam>
    /// <typeparam name="I">Type of the identifier of an entity.</typeparam>
    public interface IWriteController<T, I> : IReadController<T, I>
    {
        /// <summary>Creates a new instance.</summary>
        /// <param name="id">Identifier of newly created entity.</param>
        /// <param name="instance">Instance to be created.</param>
        /// <returns><b>true</b> for success and <b>false</b> for failure; otherwise <b>null</b> when the instance does not exist.</returns>
        bool? Create(out I id, T instance);

        /// <summary>Updates an existing instance.</summary>
        /// <param name="id">Identifier of the entity to be updated.</param>
        /// <param name="instance">Instance data.</param>
        /// <returns><b>true</b> for success and <b>false</b> for failure; otherwise <b>null</b> when the instance does not exist.</returns>
        bool? Update(I id, T instance);

        /// <summary>Deletes an entity.</summary>
        /// <param name="id">Identifier of the entity to be deleted.</param>
        /// <returns><b>true</b> for success and <b>false</b> for failure; otherwise <b>null</b> when the instance does not exist.</returns>
        bool? Delete(I id);
    }
}