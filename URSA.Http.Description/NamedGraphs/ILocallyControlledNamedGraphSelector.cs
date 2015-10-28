using System;
using RomanticWeb.NamedGraphs;

namespace URSA.Web.Http.Description.NamedGraphs
{
    /// <summary>Provides a contract for locally controller <see cref="INamedGraphSelector" /> implementation.</summary>
    public interface ILocallyControlledNamedGraphSelector : INamedGraphSelector
    {
        /// <summary>Gets or sets the named graph to be used explicitly.</summary>
        Uri NamedGraph { get; set; }
    }
}