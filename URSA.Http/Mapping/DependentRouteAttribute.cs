using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Mapping
{
    /// <summary>Binds the target of the attribute to the related controller.</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
    public class DependentRouteAttribute : RouteAttribute
    {
        /// <summary>Initializes a new instance of the <see cref="DependentRouteAttribute"/> class.</summary>
        public DependentRouteAttribute() : base("/")
        {
        }
    }
}