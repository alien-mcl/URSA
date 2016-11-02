using System;
using RomanticWeb;
using URSA.Web.Http.Description.Entities;
using VDS.RDF;

namespace URSA.Http.AutoFac.Entities
{
    /// <summary>Provides a basic implementation of the <see cref="IEntityContextProvider" /> facility.</summary>
    public class DefaultEntityContextProvider : IEntityContextProvider
    {
        /// <summary>Initializes a new instance of the <see cref="DefaultEntityContextProvider" /> class.</summary>
        /// <param name="entityContext">Entity context.</param>
        /// <param name="tripleStore">Triple store.</param>
        /// <param name="metaGraph">Meta-graph uri.</param>
        public DefaultEntityContextProvider(IEntityContext entityContext, ITripleStore tripleStore, Uri metaGraph)
        {
            EntityContext = entityContext;
            TripleStore = tripleStore;
            MetaGraph = metaGraph;
        }

        /// <inheritdoc />
        public IEntityContext EntityContext { get; private set; }

        /// <inheritdoc />
        public ITripleStore TripleStore { get; private set; }

        /// <inheritdoc />
        public Uri MetaGraph { get; private set; }
    }
}
