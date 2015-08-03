using System;

namespace URSA.Web.Http.Description.Mapping
{
    /// <summary>Represents a server-side parameter behavior.</summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class ServerBehaviorAttribute : Attribute
    {
        /// <summary>Accepts the specified visitor.</summary>
        /// <param name="visitor">The visitor.</param>
        public abstract void Accept(IServerBehaviorAttributeVisitor visitor);
    }
}