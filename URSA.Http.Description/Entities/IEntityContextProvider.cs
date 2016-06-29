using System;
using RomanticWeb;
using VDS.RDF;

namespace URSA.Web.Http.Description.Entities
{
    /// <summary>Provides a contract for an <see cref="IEntityContext" /> and associated in-memory <see cref="ITripleStore" /> instances.</summary>
    public interface IEntityContextProvider
    {
        /// <summary>Gets the entity context.</summary>
        IEntityContext EntityContext { get; }

        /// <summary>Gets the triple store.</summary>
        ITripleStore TripleStore { get; }

        /// <summary>Gets the meta graph.</summary>
        Uri MetaGraph { get; }
    }
}