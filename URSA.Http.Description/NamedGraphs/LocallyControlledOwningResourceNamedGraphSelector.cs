using System;
using System.Collections.Generic;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Model;
using URSA.Web.Description.Http;

namespace URSA.Web.Http.Description.NamedGraphs
{
    /// <summary>Default implementation of the locally controlled named graph selector.</summary>
    public class LocallyControlledOwningResourceNamedGraphSelector : OwningResourceNamedGraphSelector, ILocallyControlledNamedGraphSelector
    {
        /// <summary>Initializes a new instance of the <see cref="LocallyControlledOwningResourceNamedGraphSelector"/> class.</summary>
        /// <param name="descriptionBuilders">The description builders.</param>
        public LocallyControlledOwningResourceNamedGraphSelector(IEnumerable<IHttpControllerDescriptionBuilder> descriptionBuilders) : base(descriptionBuilders)
        {
            NamedGraph = null;
        }

        /// <inheritdoc />
        public Uri NamedGraph { get; set; }

        /// <inheritdoc />
        public override Uri SelectGraph(EntityId entityId, IEntityMapping entityMapping, IPropertyMapping predicate)
        {
            Uri result;
            if (Cache.TryGetValue(entityId, out result))
            {
                return result;
            }

            if (NamedGraph != null)
            {
                return Cache[entityId] = NamedGraph;
            }

            return base.SelectGraph(entityId, entityMapping, predicate);
        }
    }
}