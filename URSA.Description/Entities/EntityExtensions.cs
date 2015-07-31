using System;
using System.Collections.Generic;
using System.Linq;
using RomanticWeb.Entities;
using RomanticWeb.Model;
using RomanticWeb.Vocabularies;
using URSA.Web.Http.Description.Owl;
using URSA.Web.Http.Description.Rdfs;
using IClass = URSA.Web.Http.Description.Hydra.IClass;
using IResource = URSA.Web.Http.Description.Hydra.IResource;

namespace URSA.Web.Http.Description.Entities
{
    /// <summary>Provides useful <see cref="IEntity" /> extensions.</summary>
    public static class EntityExtensions
    {
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
    }
}