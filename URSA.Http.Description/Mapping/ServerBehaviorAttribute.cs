using System;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description.Mapping
{
    /// <summary>Represents a server-side parameter behavior.</summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class ServerBehaviorAttribute : Attribute
    {
        /// <summary>Accepts the specified visitor.</summary>
        /// <typeparam name="T">Type of member visited.</typeparam>
        /// <param name="visitor">The visitor.</param>
        /// <param name="templateMapping">Template mapping to be amended by visitor.</param>
        /// <param name="descriptionContext">Description context.</param>
        public abstract void Accept<T>(IServerBehaviorAttributeVisitor visitor, IIriTemplateMapping templateMapping, DescriptionContext descriptionContext);
    }
}