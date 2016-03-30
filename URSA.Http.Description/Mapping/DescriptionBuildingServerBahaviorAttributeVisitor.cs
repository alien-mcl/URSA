using System;
using System.Linq;
using System.Reflection;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description.Mapping
{
    /// <summary>Visits server behavior attributes and builds the API description.</summary>
    /// <typeparam name="TMember">Type of the member of which the attributes should be visited.</typeparam>
    public class DescriptionBuildingServerBahaviorAttributeVisitor<TMember> : IServerBehaviorAttributeVisitor where TMember : ICustomAttributeProvider
    {
        /// <summary>Exposes an OData namespace.</summary>
        public static readonly string OData = "http://docs.oasis-open.org/odata/odata/v4.0/";

        private static readonly Type[] AllowedTypes = { typeof(ParameterInfo) };

        /// <summary>Initializes a new instance of the <see cref="DescriptionBuildingServerBahaviorAttributeVisitor{TMember}" /> class.</summary>
        public DescriptionBuildingServerBahaviorAttributeVisitor()
        {
            if (!AllowedTypes.Contains(typeof(TMember)))
            {
                throw new InvalidOperationException(String.Format("Cannot visit attributes for member of type '{0}'.", typeof(TMember)));
            }
        }

        /// <inheritdoc />
        public void Visit<T>(LinqServerBehaviorAttribute behaviorAttribute, IIriTemplateMapping templateMapping, DescriptionContext descriptionContext)
        {
            IClass range = null;
            Uri uri = null;
            switch (behaviorAttribute.Operation)
            {
                case LinqOperations.Filter:
                    range = (descriptionContext.ContainsType(typeof(string)) ? descriptionContext[typeof(string)] :
                        descriptionContext.TypeDescriptionBuilder.BuildTypeDescription(descriptionContext.ForType(typeof(string))));
                    uri = new Uri(OData + "$filter");
                    break;
                case LinqOperations.Skip:
                    uri = new Uri(OData + "$skip");
                    break;
                case LinqOperations.Take:
                    uri = new Uri(OData + "$top");
                    break;
            }

            if (range == null)
            {
                range = (descriptionContext.ContainsType(typeof(T)) ? descriptionContext[typeof(T)] :
                    descriptionContext.TypeDescriptionBuilder.BuildTypeDescription(descriptionContext.ForType(typeof(T))));
            }

            templateMapping.Property = templateMapping.Context.Create<Rdfs.IProperty>(uri);
            templateMapping.Property.Range.Add(range);
        }
    }
}