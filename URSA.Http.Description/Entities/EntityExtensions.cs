using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RomanticWeb;
using RomanticWeb.Configuration;
using RomanticWeb.DotNetRDF;
using RomanticWeb.Entities;
using RomanticWeb.Model;
using RomanticWeb.NamedGraphs;
using RomanticWeb.Vocabularies;
using URSA.Configuration;
using URSA.Web.Http.Description.Owl;
using URSA.Web.Http.Description.Rdfs;
using VDS.RDF;
using IClass = URSA.Web.Http.Description.Hydra.IClass;
using IResource = URSA.Web.Http.Description.Hydra.IResource;

namespace URSA.Web.Http.Description.Entities
{
    /// <summary>Provides useful <see cref="IEntity" /> extensions.</summary>
    public static class EntityExtensions
    {
        /// <summary>Renames the specified entity with a new IRI.</summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="newId">New entity identifier.</param>
        /// <returns>Copy of the <paramref name="entity" /> with new identifier.</returns>
        public static T Rename<T>(this T entity, EntityId newId) where T : class, IEntity
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (newId == null)
            {
                throw new ArgumentNullException("newId");
            }

            var entitySourceField = entity.Context.GetType().GetField("_entitySource", BindingFlags.Instance | BindingFlags.NonPublic);
            if (entitySourceField == null)
            {
                throw new InvalidOperationException("Unknown type of entity context encountered.");
            }

            var namedGraphSelector = ((IEntityContextFactory)entity.Context.GetType().GetField("_factory", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(entity.Context)).NamedGraphSelector;
            var entitySource = (IEntitySource)entitySourceField.GetValue(entity.Context);
            if (!(entitySource is TripleStoreAdapter))
            {
                throw new InvalidOperationException("Uknown entity source encountered.");
            }

            var store = (ITripleStore)entitySource.GetType().GetField("_store", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(entitySource);
            if ((!(store is TripleStore)) && (!(store is ThreadSafeTripleStore)))
            {
                throw new InvalidOperationException("Uknown triple store encountered.");
            }

            if (store.Graphs.Any(graph => AbsoluteUriComparer.Default.Equals(graph.BaseUri, newId.Uri)))
            {
                throw new InvalidOperationException(String.Format("Entity with IRI of <{0}> already exists.", newId));
            }

            var metaGraph = store.Graphs[ConfigurationSectionHandler.Default.Factories[DescriptionConfigurationSection.Default.DefaultStoreFactoryName].MetaGraphUri];
            var newGraph = (store is ThreadSafeTripleStore ? new ThreadSafeGraph() : new Graph());
            var graphNode = metaGraph.CreateUriNode(newGraph.BaseUri = namedGraphSelector.SelectGraph(newId, null, null));
            var primaryTopicNode = metaGraph.CreateUriNode(Foaf.primaryTopic);
            store.Add(newGraph);
            foreach (var triple in store.Graphs.First(graph => AbsoluteUriComparer.Default.Equals(graph.BaseUri, entity.Id.Uri)).Triples)
            {
                INode subject;
                newGraph.Assert(
                    subject = triple.Subject.Clone(newGraph, entity.Id.Uri, newId.Uri),
                    triple.Predicate.CopyNode(newGraph),
                    triple.Object.Clone(newGraph, entity.Id.Uri, newId.Uri));
                if (subject is IUriNode)
                {
                    metaGraph.Assert(graphNode, primaryTopicNode, metaGraph.CreateUriNode(((IUriNode)subject).Uri));
                }
            }

            return entity.Context.Load<T>(newId);
        }

        /// <summary>Clones a given blank node.</summary>
        /// <remarks>This method expect <paramref name="source" /> to be a blank node. If it's not the case, original instance will be returned.</remarks>
        /// <typeparam name="T">Type of the entity being cloned.</typeparam>
        /// <param name="source">The source entity.</param>
        /// <returns>New copy of the given <paramref name="source" />.</returns>
        public static T Clone<T>(this T source) where T : class, IEntity
        {
            if (!(source.Id is BlankId))
            {
                return source;
            }

            var blankId = (BlankId)source.Id;
            if (blankId.RootEntityId == null)
            {
                throw new InvalidOperationException("No root entity was found.");
            }

            var newBlankId = source.Context.Load<IEntity>(blankId.RootEntityId).CreateBlankId();
            var result = source.Context.Create<T>(newBlankId);
            var quads = from quad in source.Context.Store.Quads
                        where (quad.Subject.IsBlank) && (quad.Subject.ToEntityId() == blankId)
                        let subject = Node.ForBlank(newBlankId.Identifier, newBlankId.RootEntityId, newBlankId.Graph)
                        select new EntityQuad(newBlankId, subject, quad.Predicate, quad.Object, quad.Graph);
            result.Context.Store.AssertEntity(result.Id, quads);
            return result;
        }

        internal static bool IsGenericRdfList(this IClass @class, out IResource itemRestriction)
        {
            bool result = false;
            itemRestriction = null;
            foreach (var superClass in @class.SubClassOf)
            {
                if (superClass.Is(RomanticWeb.Vocabularies.Rdf.List))
                {
                    result = true;
                }

                IRestriction restriction = null;
                if ((superClass.Is(RomanticWeb.Vocabularies.Owl.Restriction)) &&
                    ((restriction = superClass.AsEntity<IRestriction>()).OnProperty != null) && (restriction.OnProperty.Id == RomanticWeb.Vocabularies.Rdf.first) &&
                    (restriction.AllValuesFrom != null) && (restriction.AllValuesFrom.Is(RomanticWeb.Vocabularies.Rdfs.Resource)))
                {
                    itemRestriction = restriction.AllValuesFrom.AsEntity<IResource>();
                }
            }

            return result;
        }

        internal static IEnumerable<Rdfs.IResource> GetUniqueIdentifierType(this IClass @class)
        {
            return (from supportedProperty in @class.SupportedProperties
                    let property = supportedProperty.Property
                    where property.Is(RomanticWeb.Vocabularies.Owl.InverseFunctionalProperty)
                    select property.Range).FirstOrDefault() ?? new Rdfs.IResource[0];
        }

        internal static IRestriction CreateRestriction(this IResource resource, Uri onProperty, IEntity allValuesFrom)
        {
            var typeConstrain = resource.Context.Create<IRestriction>(resource.CreateBlankId());
            typeConstrain.OnProperty = resource.Context.Create<IProperty>(new EntityId(onProperty));
            typeConstrain.AllValuesFrom = allValuesFrom;
            return typeConstrain;
        }

        internal static IRestriction CreateRestriction(this IResource resource, IProperty onProperty, uint maxCardinality)
        {
            var typeConstrain = resource.Context.Create<IRestriction>(resource.CreateBlankId());
            typeConstrain.OnProperty = onProperty;
            typeConstrain.MaxCardinality = maxCardinality;
            return typeConstrain;
        }

        private static INode Clone(this INode node, IGraph targetGraph, Uri currentId, Uri newId)
        {
            if (!(node is IUriNode))
            {
                return node.CopyNode(targetGraph);
            }

            IUriNode uriNode = (IUriNode)node;
            return (AbsoluteUriComparer.Default.Equals(uriNode.Uri, currentId) ? targetGraph.CreateUriNode(newId) : uriNode.CopyNode(targetGraph));
        }
    }
}