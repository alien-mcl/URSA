using System.Collections.Generic;
using RomanticWeb.Entities;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description.Mapping
{
    /// <summary>Provides a description of the <see cref="ServerBehaviorAttribute" /> visitor facility.</summary>
    public interface IServerBehaviorAttributeVisitor
    {
        /// <summary>Gets or sets the API documentation.</summary>
        /// <value>The API documentation.</value>
        IApiDocumentation ApiDocumentation { get; set; }

        /// <summary>Gets the member description.</summary>
        IEntity MemberDescription { get; }

        /// <summary>Visits the LINQ behavior attribute.</summary>
        /// <param name="behaviorAttribute">The behavior attribute.</param>
        void Visit(LinqServerBehaviorAttribute behaviorAttribute);
    }

    /// <summary>Provides a simple visitor hookup routine.</summary>
    public static class ServerBahaviorAttributeExtensions
    {
        /// <summary>Accepts the specified visitor.</summary>
        /// <param name="attributes">The attributes to be visited.</param>
        /// <param name="visitor">The visitor.</param>
        public static void Accept(this IEnumerable<ServerBehaviorAttribute> attributes, IServerBehaviorAttributeVisitor visitor)
        {
            foreach (var attribute in attributes)
            {
                attribute.Accept(visitor);
            }
        }
    }
}