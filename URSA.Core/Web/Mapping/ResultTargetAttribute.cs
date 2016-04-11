using System;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Mapping
{
    /// <summary>Determines target of the resulting argument.</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter)]
    public abstract class ResultTargetAttribute : MappingAttribute
    {
    }
}