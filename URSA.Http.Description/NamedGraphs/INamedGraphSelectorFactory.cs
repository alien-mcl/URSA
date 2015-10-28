using RomanticWeb.NamedGraphs;

namespace URSA.Web.Http.Description.NamedGraphs
{
    /// <summary>Provides a contract of a <see cref="INamedGraphSelector" /> factory.</summary>
    public interface INamedGraphSelectorFactory
    {
        /// <summary>Gets the instance of the <see cref="INamedGraphSelector" /> implementation.</summary>
        INamedGraphSelector NamedGraphSelector { get; }
    }
}