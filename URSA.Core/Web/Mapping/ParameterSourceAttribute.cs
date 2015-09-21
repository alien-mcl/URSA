using System;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Mapping
{
    /// <summary>Parameter source marker.</summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class ParameterSourceAttribute : MappingAttribute
    {
    }
}