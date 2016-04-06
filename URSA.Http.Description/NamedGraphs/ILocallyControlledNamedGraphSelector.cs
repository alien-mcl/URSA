using System;
using RomanticWeb.Entities;
using RomanticWeb.NamedGraphs;

namespace URSA.Web.Http.Description.NamedGraphs
{
    /// <summary>Provides a contract for locally controller <see cref="INamedGraphSelector" /> implementation.</summary>
    public interface ILocallyControlledNamedGraphSelector : INamedGraphSelector
    {
        /// <summary>Gets or sets the named graph to be used explicitly.</summary>
        Uri NamedGraph { get; set; }

        /// <summary>Gets or sets a delegate used to obtain a current context request.</summary>
        Func<IRequestInfo> CurrentRequest { get; set; } 

        /// <summary>Maps the entity graph for a given request.</summary>
        /// <param name="request">The request.</param>
        /// <param name="entityId">The entity IRI.</param>
        /// <param name="namedGraph">The named graph.</param>
        void MapEntityGraphForRequest(IRequestInfo request, EntityId entityId, Uri namedGraph);

        /// <summary>Removes the entity graph mapping for a given request.</summary>
        /// <param name="request">The request.</param>
        /// <param name="entityId">The entity IRI.</param>
        void UnmapEntityGraphForRequest(IRequestInfo request, EntityId entityId);
    }
}