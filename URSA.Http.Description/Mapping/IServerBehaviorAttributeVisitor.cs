using System;
using System.Collections.Generic;
using System.Reflection;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description.Mapping
{
    /// <summary>Provides a description of the <see cref="ServerBehaviorAttribute" /> visitor facility.</summary>
    public interface IServerBehaviorAttributeVisitor
    {
        /// <summary>Visits the LINQ behavior attribute.</summary>
        /// <typeparam name="T">Type of the member visited.</typeparam>
        /// <param name="behaviorAttribute">The behavior attribute.</param>
        /// <param name="templateMapping">Template mapping to be amended by visitor.</param>
        /// <param name="descriptionContext">Description context.</param>
        void Visit<T>(LinqServerBehaviorAttribute behaviorAttribute, IIriTemplateMapping templateMapping, DescriptionContext descriptionContext);
    }

    /// <summary>Provides a simple visitor hookup routine.</summary>
    public static class ServerBahaviorAttributeExtensions
    {
        /// <summary>Accepts the specified visitor.</summary>
        /// <param name="attributes">The attributes to be visited.</param>
        /// <param name="memberType">Type of the member.</param>
        /// <param name="visitor">The visitor.</param>
        /// <param name="templateMapping">Template mapping to be amended by visitor.</param>
        /// <param name="descriptionContext">Description context.</param>
        public static void Accept(
            this IEnumerable<ServerBehaviorAttribute> attributes,
            Type memberType,
            IServerBehaviorAttributeVisitor visitor, 
            IIriTemplateMapping templateMapping, 
            DescriptionContext descriptionContext)
        {
            foreach (var attribute in attributes)
            {
                attribute.GetType().GetMethod("Accept", BindingFlags.Instance | BindingFlags.Public)
                    .MakeGenericMethod(memberType).Invoke(attribute, new object[] { visitor, templateMapping, descriptionContext });
            }
        }
    }
}