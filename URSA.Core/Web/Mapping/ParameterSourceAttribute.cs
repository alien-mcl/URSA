using System;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Mapping
{
    /// <summary>Parameter source marker.</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class ParameterSourceAttribute : MappingAttribute
    {
    }
}