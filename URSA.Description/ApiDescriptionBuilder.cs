using RomanticWeb.Entities;
using RomanticWeb.Model;
using RomanticWeb.Vocabularies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RomanticWeb.Mapping.Attributes;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Description.CodeGen;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Owl;
using URSA.Web.Mapping;
using IClass = URSA.Web.Http.Description.Hydra.IClass;

namespace URSA.Web.Http.Description
{
    /// <summary>Builds API description.</summary>
    /// <typeparam name="T">Type of the API to describe</typeparam>
    public class ApiDescriptionBuilder<T> where T : IController
    {
        /// <summary>The default entity context factory name.</summary>
        public const string DefaultEntityContextFactoryName = "http";

        internal const string DotNetSymbol = "net";
        internal const string DotNetEnumerableSymbol = "net-enumerable";
        internal const string DotNetListSymbol = "net-list";
        internal const string DotNetCollectionSymbol = "net-collection";
        internal const string HydraSymbol = "hydra";

        private static readonly IDictionary<Type, Uri> TypeDescriptions = XsdUriParser.Types.Concat(OGuidUriParser.Types).ToDictionary(item => item.Key, item => item.Value);

        private readonly IHttpControllerDescriptionBuilder<T> _descriptionBuilder;
        private Type _specializationType;

        /// <summary>Initializes a new instance of the <see cref="ApiDescriptionBuilder{T}" /> class.</summary>
        /// <param name="descriptionBuilder">Description builder.</param>
        public ApiDescriptionBuilder(IHttpControllerDescriptionBuilder<T> descriptionBuilder)
        {
            if (descriptionBuilder == null)
            {
                throw new ArgumentNullException("descriptionBuilder");
            }

            _descriptionBuilder = descriptionBuilder;
        }

        private Type SpecializationType
        {
            get
            {
                if (_specializationType == null)
                {
                    _specializationType = (from @interface in typeof(T).GetInterfaces()
                                           where (@interface.IsGenericType) && (typeof(IController<>).IsAssignableFrom(@interface.GetGenericTypeDefinition()))
                                           from type in @interface.GetGenericArguments()
                                           select type).FirstOrDefault() ?? typeof(object);
                }

                return _specializationType;
            }
        }

        /// <summary>Builds an API description.</summary>
        /// <param name="apiDocumentation">API documentation.</param>
        public void BuildDescription(IApiDocumentation apiDocumentation)
        {
            if (apiDocumentation == null)
            {
                throw new ArgumentNullException("apiDocumentation");
            }

            Uri baseUri = apiDocumentation.Context.BaseUriSelector.SelectBaseUri(new EntityId(new Uri("/", UriKind.Relative)));
            IDictionary<Type, Rdfs.IResource> typeDefinitions = new Dictionary<Type, Rdfs.IResource>();
            var description = _descriptionBuilder.BuildDescriptor();
            if (SpecializationType == typeof(object))
            {
                return;
            }

            IClass specializationType = (IClass)(typeDefinitions[SpecializationType] = BuildTypeDescription(apiDocumentation, SpecializationType, typeDefinitions));
            apiDocumentation.SupportedClasses.Add(specializationType);
            foreach (OperationInfo<Verb> operation in description.Operations)
            {
                IIriTemplate template;
                var operationDefinition = BuildOperation(apiDocumentation, baseUri, operation, typeDefinitions, out template);
                if (template != null)
                {
                    ITemplatedLink templatedLink = apiDocumentation.Context.Create<ITemplatedLink>(template.Id.Uri.AbsoluteUri.Replace("#template", "#withTemplate"));
                    templatedLink.Operations.Add(operationDefinition);
                    apiDocumentation.Context.Store.ReplacePredicateValues(
                        specializationType.Id,
                        Node.ForUri(templatedLink.Id.Uri),
                        () => new Node[] { Node.ForUri(template.Id.Uri) },
                        specializationType.Id.Uri);
                }
                else
                {
                    specializationType.SupportedOperations.Add(operationDefinition);
                }
            }
        }

        private IOperation BuildOperation(IApiDocumentation apiDocumentation, Uri baseUri, OperationInfo<Verb> operation, IDictionary<Type, Rdfs.IResource> typeDefinitions, out IIriTemplate template)
        {
            EntityId methodId = new EntityId(!operation.Arguments.Any() ? operation.Uri.Combine(baseUri) :
                operation.Uri.Combine(baseUri).AddFragment(String.Format(
                "{0}{1}",
                operation.ProtocolSpecificCommand,
                String.Join("And", operation.Arguments.Select(argument => (argument.VariableName ?? argument.Parameter.Name).ToUpperCamelCase())))));
            IOperation result = apiDocumentation.Context.Create<IOperation>(methodId);
            result.Label = operation.UnderlyingMethod.Name;
            result.Method.Add(_descriptionBuilder.GetMethodVerb(operation.UnderlyingMethod).ToString());
            template = BuildTemplate(apiDocumentation, operation, result, typeDefinitions);
            Type type;
            if (((type = operation.UnderlyingMethod.DeclaringType.GetInterfaces()
                .FirstOrDefault(@interface => (@interface.IsGenericType) && (typeof(IWriteController<,>).IsAssignableFrom(@interface.GetGenericTypeDefinition())))) != null) &&
                (operation.UnderlyingMethod.DeclaringType.GetInterfaceMap(type).TargetMethods.Contains(operation.UnderlyingMethod)))
            {
                var parameters = operation.UnderlyingMethod.GetParameters();
                if (parameters.Length > 1)
                {
                    result.Expects.Add(BuildTypeDescription(apiDocumentation, parameters[1].ParameterType, typeDefinitions));
                }
            }
            else
            {
                foreach (var parameter in operation.Arguments.Where(parameter => parameter.Source is FromBodyAttribute))
                {
                    result.Expects.Add(BuildTypeDescription(apiDocumentation, parameter.Parameter.ParameterType, typeDefinitions));
                }

                foreach (var output in operation.Results.Where(output => output.Target is ToBodyAttribute))
                {
                    result.Returns.Add(BuildTypeDescription(apiDocumentation, output.Parameter.ParameterType, typeDefinitions));
                }
            }

            return result;
        }

        private IIriTemplate BuildTemplate(IApiDocumentation apiDocumentation, OperationInfo<Verb> operation, IOperation operationDocumentation, IDictionary<Type, Rdfs.IResource> typeDefinitions)
        {
            IIriTemplate template = null;
            IEnumerable<ArgumentInfo> parameterMapping;
            var uriTemplate = _descriptionBuilder.GetOperationUriTemplate(operation.UnderlyingMethod, out parameterMapping);
            if (!String.IsNullOrEmpty(uriTemplate))
            {
                foreach (var mapping in parameterMapping)
                {
                    if (template == null)
                    {
                        template = apiDocumentation.Context.Create<IIriTemplate>(operationDocumentation.Id.Uri.AddFragment("template"));
                        template.Template = uriTemplate;
                    }

                    if (mapping.Source is FromBodyAttribute)
                    {
                        continue;
                    }

                    IIriTemplateMapping templateMapping = apiDocumentation.Context.Create<IIriTemplateMapping>(template.Id.Uri.AddFragment(mapping.VariableName));
                    templateMapping.Variable = mapping.VariableName;
                    templateMapping.Required = mapping.Parameter.ParameterType.IsValueType;
                    if (SpecializationType != typeof(object))
                    {
                        templateMapping.Property = GetMappingProperty(apiDocumentation, mapping.Parameter, SpecializationType, typeDefinitions);
                    }

                    template.Mappings.Add(templateMapping);
                }
            }

            return template;
        }

        private Rdfs.IProperty GetMappingProperty(IApiDocumentation apiDocumentation, ParameterInfo parameter, Type type, IDictionary<Type, Rdfs.IResource> typeDefinitions)
        {
            Rdfs.IResource @class;
            if (!typeDefinitions.TryGetValue(type, out @class))
            {
                @class = BuildTypeDescription(apiDocumentation, type, typeDefinitions);
            }

            if (@class is IClass)
            {
                return (from supportedProperty in ((IClass)@class).SupportedProperties
                        let property = supportedProperty.Property
                        where StringComparer.OrdinalIgnoreCase.Equals(property.Label, parameter.Name)
                        select property).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        private IClass BuildTypeDescription(IApiDocumentation apiDocumentation, Type type, IDictionary<Type, Rdfs.IResource> typeDefinitions)
        {
            Uri uri;
            if (TypeDescriptions.TryGetValue(type, out uri))
            {
                var datatype = apiDocumentation.Context.Create<IResource>(new EntityId(uri));
                var definition = apiDocumentation.Context.Create<IDatatypeDefinition>(new EntityId(String.Format("urn:guid:{0}", Guid.NewGuid())));
                definition.Datatype = datatype;
                return definition.AsEntity<IClass>();
            }

            if (type.IsList())
            {
                return (IClass)(typeDefinitions[type] = CreateListDefinition(apiDocumentation, type, typeDefinitions, type.IsGenericList()));
            }

            IClass result = null;
            Type itemType = type.FindItemType();
            result = apiDocumentation.Context.Create<IClass>(new EntityId(new Uri(String.Format("urn:" + DotNetSymbol + ":{0}", itemType))));
            result.Label = type.Name;
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var supportedProperty = apiDocumentation.Context.Create<ISupportedProperty>(new Uri(String.Format("urn:" + HydraSymbol + ":{0}.{1}", itemType, property.Name)));
                supportedProperty.ReadOnly = !property.CanWrite;
                supportedProperty.WriteOnly = !property.CanRead;
                supportedProperty.Required = property.PropertyType.IsValueType;
                supportedProperty.Property = (property.GetCustomAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>() != null ? 
                    apiDocumentation.Context.Create<Owl.IInverseFunctionalProperty>(result.Id.Uri.AddName(property.Name)) :
                    apiDocumentation.Context.Create<Rdfs.IProperty>(result.Id.Uri.AddName(property.Name)));
                supportedProperty.Property.Label = property.Name;
                supportedProperty.Property.Domain.Add(result);
                Rdfs.IResource range;
                if (!typeDefinitions.TryGetValue(property.PropertyType, out range))
                {
                    typeDefinitions[property.PropertyType] = range = BuildTypeDescription(apiDocumentation, property.PropertyType, typeDefinitions);
                }

                supportedProperty.Property.Range.Add(range);

                if ((!System.Reflection.TypeExtensions.IsEnumerable(property.PropertyType)) || (property.PropertyType == typeof(byte[])))
                {
                    var restriction = apiDocumentation.Context.Create<IRestriction>(result.CreateBlankId());
                    restriction.OnProperty = supportedProperty.Property;
                    restriction.MaxCardinality = 1u;
                    result.SubClassOf.Add(restriction);
                }

                result.SupportedProperties.Add(supportedProperty);
            }

            return (IClass)(typeDefinitions[type] = result);
        }

        private IClass CreateListDefinition(IApiDocumentation apiDocumentation, Type type, IDictionary<Type, Rdfs.IResource> typeDefinitions, bool isGeneric = true)
        {
            if (!isGeneric)
            {
                return apiDocumentation.Context.Create<IClass>(new EntityId(Rdf.List));
            }

            var itemType = type.GetItemType();
            var result = apiDocumentation.Context.Create<IClass>(new EntityId(new Uri(String.Format("urn:" + DotNetListSymbol + ":{0}", itemType.FullName))));
            result.Label = type.Name;
            result.SubClassOf.Add(apiDocumentation.Context.Create<IClass>(new EntityId(Rdf.List)));

            var typeConstrain = apiDocumentation.Context.Create<Owl.IRestriction>(result.CreateBlankId());
            typeConstrain.OnProperty = apiDocumentation.Context.Create<Rdfs.IProperty>(new EntityId(Rdf.first));
            typeConstrain.AllValuesFrom = typeDefinitions.GetOrCreate(itemType, () => BuildTypeDescription(apiDocumentation, itemType, typeDefinitions));
            result.SubClassOf.Add(typeConstrain);

            var restConstrain = apiDocumentation.Context.Create<Owl.IRestriction>(result.CreateBlankId());
            restConstrain.OnProperty = apiDocumentation.Context.Create<Rdfs.IProperty>(new EntityId(Rdf.rest));
            restConstrain.AllValuesFrom = result;
            result.SubClassOf.Add(restConstrain);
            return result;
        }
    }
}