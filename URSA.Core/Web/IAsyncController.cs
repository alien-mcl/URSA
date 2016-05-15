using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace URSA.Web
{
    /// <summary>Specialized asynchronous controller focused on CRUD operations of type <typeparamref name="T" />.</summary>
    /// <typeparam name="T">Type of resource given controller works with.</typeparam>
    public interface IAsyncController<T> : IController
    {
        /// <summary>Gets all </summary>
        /// <param name="totalItems">Number of total items in the collection if <paramref name="skip" /> and <paramref name="take" /> are used.</param>
        /// <param name="skip">Skips top <paramref name="skip" /> elements of the collection.</param>
        /// <param name="take">Takes top <paramref name="take" /> elements of the collection. Use 0 for all of the entities.</param>
        /// <param name="filter">Expression to be used for entity filtering.</param>
        /// <returns>Collection of entities.</returns>
        Task<IEnumerable<T>> ListAsync(out int totalItems, int skip = 0, int take = 0, Expression<Func<T, bool>> filter = null);
    }
}