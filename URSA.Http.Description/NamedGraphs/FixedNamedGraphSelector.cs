using System;
using System.Diagnostics.CodeAnalysis;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Model;
using RomanticWeb.NamedGraphs;

namespace URSA.Web.Http.Description.NamedGraphs
{
    /// <summary>Provides a fixed named graph selector.</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
    public class FixedNamedGraphSelector : INamedGraphSelector
    {
        private static readonly Uri GraphUri = new Uri("graph:ursa:meta");

        /// <inheritdoc />
        public Uri SelectGraph(EntityId entityId, IEntityMapping entityMapping, IPropertyMapping predicate)
        {
            return GraphUri;
        }
    }
}