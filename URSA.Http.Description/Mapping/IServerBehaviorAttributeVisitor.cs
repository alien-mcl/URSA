using System.Collections.Generic;
using RomanticWeb.Entities;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description.Mapping
{
    /// <summary>Provides a description of the <see cref="ServerBehaviorAttribute" /> visitor facility.</summary>
    public interface IServerBehaviorAttributeVisitor
    {
        /// <summary>Visits the LINQ behavior attribute.</summary>
        /// <param name="behaviorAttribute">The behavior attribute.</param>
        /// <param name="templateMapping">Template mapping to be amended by visitor.</param>
        void Visit(LinqServerBehaviorAttribute behaviorAttribute, IIriTemplateMapping templateMapping);
    }

    /// <summary>Provides a simple visitor hookup routine.</summary>
    public static class ServerBahaviorAttributeExtensions
    {
        /// <summary>Accepts the specified visitor.</summary>
        /// <param name="attributes">The attributes to be visited.</param>
        /// <param name="visitor">The visitor.</param>
        /// <param name="templateMapping">Template mapping to be amended by visitor.</param>
        public static void Accept(this IEnumerable<ServerBehaviorAttribute> attributes, IServerBehaviorAttributeVisitor visitor, IIriTemplateMapping templateMapping)
        {
            foreach (var attribute in attributes)
            {
                attribute.Accept(visitor, templateMapping);
            }
        }
    }
}