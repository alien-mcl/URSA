using System.Collections.Generic;

namespace URSA.Web
{
    /// <summary>Marks classes as request handlers.</summary>
    public interface IController
    {
        /// <summary>Gets or sets the response descriptor.</summary>
        IResponseInfo Response { get; set; }
    }

    /// <summary>Specialized controller focused on CRUD operations of type <typeparamref name="T" />.</summary>
    /// <typeparam name="T">Type of resource given controller works with.</typeparam>
    public interface IController<T> : IController
    {
        /// <summary>Gets all </summary>
        /// <param name="page">Page of the collection. Use 0 for all of the entities.</param>
        /// <param name="pageSize">Page size. Ignored when <paramref name="page" /> is set to 0.</param>
        /// <returns>Collection of entities.</returns>
        IEnumerable<T> Get(int page = 0, int pageSize = 0);
    }
}