using System;
using RomanticWeb;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using URSA.Web.Http.Description.Rdfs;

namespace URSA.Web.Http.Description.Shacl
{
    /// <summary>Describes a RDF node kind.</summary>
    [Class("sh", "NodeKind")]
    public interface INodeKind : IClass
    {
    }

    /// <summary>Provides methods for accessing <see cref="INodeKind" /> named instances.</summary>
    public static class NodeKinds
    {
        /// <summary>Obtains the BlankNode node kind.</summary>
        /// <param name="entityContext">The entity context.</param>
        /// <returns>Instance of the <see cref="INodeKind" />.</returns>
        public static INodeKind BlankNode(this IEntityContext entityContext)
        {
            return entityContext.CreateNamedInstance("BlankNode");
        }

        /// <summary>Obtains the IRI node kind.</summary>
        /// <param name="entityContext">The entity context.</param>
        /// <returns>Instance of the <see cref="INodeKind" />.</returns>
        public static INodeKind IRI(this IEntityContext entityContext)
        {
            return entityContext.CreateNamedInstance("IRI");
        }

        /// <summary>Obtains the Literal node kind.</summary>
        /// <param name="entityContext">The entity context.</param>
        /// <returns>Instance of the <see cref="INodeKind" />.</returns>
        public static INodeKind Literal(this IEntityContext entityContext)
        {
            return entityContext.CreateNamedInstance("Literal");
        }

        private static INodeKind CreateNamedInstance(this IEntityContext entityContext, string term)
        {
            if (entityContext == null)
            {
                throw new ArgumentNullException("entityContext");
            }

            return entityContext.Create<INodeKind>(new EntityId(entityContext.Ontologies.ResolveUri("sp", term)));
        }
    }
}