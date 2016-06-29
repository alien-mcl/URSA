using System;
using System.Collections.Generic;
using URSA.Web;

namespace URSA.Example.WebApplication.Data
{
    /// <summary>Describes an abstract persistable entity repository.</summary>
    /// <typeparam name="TEntity">Type of the entiti stored.</typeparam>
    /// <typeparam name="TId">Type of the entity identifier.</typeparam>
    public interface IPersistingRepository<TEntity, TId> where TEntity : class, IControlledEntity<TId>
    {
        /// <summary>Gets the total count of entities.</summary>
        int Count { get; }

        /// <summary>Gets all the entities.</summary>
        /// <param name="skip">Specifies how many entities to skip.</param>
        /// <param name="take">Specifies how many entities to take.</param>
        /// <param name="predicate">Specifies additiona entity filtering.</param>
        /// <returns>Enumeration of entities.</returns>
        IEnumerable<TEntity> All(int skip = 0, int take = 0, Func<TEntity, bool> predicate = null);

        /// <summary>Gets the entity specifiad by the <paramref name="id" />.</summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>Entity with matching <paramref name="id" /> or <b>null</b>.</returns>
        TEntity Get(TId id);

        /// <summary>Creates a <paramref name="entity" />.</summary>
        /// <param name="entity">The entity.</param>
        void Create(TEntity entity);

        /// <summary>Updates a <paramref name="entity" />.</summary>
        /// <param name="entity">The entity.</param>
        void Update(TEntity entity);

        /// <summary>Deletes the entity specified by the <paramref name="id" />.</summary>
        /// <param name="id">The entity identifier.</param>
        void Delete(TId id);
    }
}
