using System;

namespace URSA.Web.Http.Description.Mapping
{
    /// <summary>Represents a server-side parameter behavior.</summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class ServerBehaviorAttribute : Attribute
    {
    }
}