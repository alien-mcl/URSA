using System;
using System.Linq;
using System.Reflection;
using RomanticWeb.Entities;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description.Mapping
{
    /// <summary>Visits server behavior attributes and builds the API description.</summary>
    /// <typeparam name="T">Type of the member of which the attributes should be visited.</typeparam>
    public class DescriptionBuildingServerBahaviorAttributeVisitor<T> : IServerBehaviorAttributeVisitor where T : ICustomAttributeProvider
    {
        private static readonly Type[] AllowedTypes = new[] { typeof(ParameterInfo) };

        /// <summary>Initializes a new instance of the <see cref="DescriptionBuildingServerBahaviorAttributeVisitor{T}" /> class.</summary>
        public DescriptionBuildingServerBahaviorAttributeVisitor()
        {
            if (!AllowedTypes.Contains(typeof(T)))
            {
                throw new InvalidOperationException(String.Format("Cannot visit attributes for member of type '{0}'.", typeof(T)));
            }
        }

        /// <inheritdoc />
        public void Visit(LinqServerBehaviorAttribute behaviorAttribute, IIriTemplateMapping templateMapping)
        {
            templateMapping.Property = templateMapping.Context.Create<Rdfs.IProperty>(
                DescriptionController<IController>.VocabularyBaseUri.AbsoluteUri + behaviorAttribute.Operation.ToString().ToLowerCamelCase());
        }
    }
}