using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Model;
using URSA.Web.Description.Http;

namespace URSA.Web.Http.Description.NamedGraphs
{
    //// TODO: Consider removing custom named graph selectors
    /// <summary>Default implementation of the locally controlled named graph selector.</summary>
    public class LocallyControlledOwningResourceNamedGraphSelector : OwningResourceNamedGraphSelector, ILocallyControlledNamedGraphSelector
    {
        private readonly IDictionary<IRequestInfo, IDictionary<EntityId, Uri>> _requestMappings;

        /// <summary>Initializes a new instance of the <see cref="LocallyControlledOwningResourceNamedGraphSelector"/> class.</summary>
        /// <param name="descriptionBuilders">The description builders.</param>
        public LocallyControlledOwningResourceNamedGraphSelector(IEnumerable<IHttpControllerDescriptionBuilder> descriptionBuilders) : base(descriptionBuilders)
        {
            NamedGraph = null;
            _requestMappings = new ConcurrentDictionary<IRequestInfo, IDictionary<EntityId, Uri>>();
        }

        /// <inheritdoc />
        public Uri NamedGraph { get; set; }

        /// <inheritdoc />
        public Func<IRequestInfo> CurrentRequest { get; set; }

        /// <inheritdoc />
        public void MapEntityGraphForRequest(IRequestInfo request, EntityId entityId, Uri namedGraph)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (entityId == null)
            {
                throw new ArgumentNullException("entityId");
            }

            if (namedGraph == null)
            {
                throw new ArgumentNullException("namedGraph");
            }

            IDictionary<EntityId, Uri> requestMap;
            if (!_requestMappings.TryGetValue(request, out requestMap))
            {
                _requestMappings[request] = requestMap = new ConcurrentDictionary<EntityId, Uri>();
            }

            requestMap[entityId] = namedGraph;
        }

        /// <inheritdoc />
        public void UnmapEntityGraphForRequest(IRequestInfo request, EntityId entityId)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (entityId == null)
            {
                throw new ArgumentNullException("entityId");
            }

            IDictionary<EntityId, Uri> requestMap;
            if (!_requestMappings.TryGetValue(request, out requestMap))
            {
                return;
            }

            requestMap.Remove(entityId);
            if (requestMap.Count == 0)
            {
                _requestMappings.Remove(request);
            }
        }

        /// <inheritdoc />
        public override Uri SelectGraph(EntityId entityId, IEntityMapping entityMapping, IPropertyMapping predicate)
        {
            Uri result;
            var request = (CurrentRequest != null ? CurrentRequest() : null);
            if (request != null)
            {
                IDictionary<EntityId, Uri> requestMap;
                if ((_requestMappings.TryGetValue(request, out requestMap)) && (requestMap.TryGetValue(entityId, out result)))
                {
                    return result;
                }
            }

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