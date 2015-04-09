using RomanticWeb.Entities;
using RomanticWeb.Model;
using RomanticWeb.Vocabularies;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using RomanticWeb.Mapping.Attributes;
using URSA.Web.Converters;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description.CodeGen;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Mapping;
using URSA.Web.Http.Description.Owl;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;
using IClass = URSA.Web.Http.Description.Hydra.IClass;

namespace URSA.Web.Http.Description
{
    /// <summary>Builds API description.</summary>
    /// <typeparam name="T">Type of the API to describe</typeparam>
    public class ApiDescriptionBuilder<T> where T : IController
    {
        internal const string DotNetSymbol = "net";
        internal const string DotNetEnumerableSymbol = "net-enumerable";
        internal const string DotNetListSymbol = "net-list";
        internal const string DotNetCollectionSymbol = "net-collection";
        internal const string HydraSymbol = "hydra";

        private static readonly IDictionary<Type, Uri> TypeDescriptions = XsdUriParser.Types.Concat(OGuidUriParser.Types).ToDictionary(item => item.Key, item => item.Value);

        private readonly IHttpControllerDescriptionBuilder<T> _descriptionBuilder;
        private readonly IXmlDocProvider _xmlDocProvider;
        private Type _specializationType;

        /// <summary>Initializes a new instance of the <see cref="ApiDescriptionBuilder{T}" /> class.</summary>
        /// <param name="descriptionBuilder">Description builder.</param>
        /// <param name="xmlDocProvider">The XML documentation provider.</param>
        public ApiDescriptionBuilder(IHttpControllerDescriptionBuilder<T> descriptionBuilder, IXmlDocProvider xmlDocProvider)
        {
            if (descriptionBuilder == null)
            {
                throw new ArgumentNullException("descriptionBuilder");
            }

            if (xmlDocProvider == null)
            {
                throw new ArgumentNullException("xmlDocProvider");
            }

            _descriptionBuilder = descriptionBuilder;
            _xmlDocProvider = xmlDocProvider;
        }

        private Type SpecializationType
        {
            get
            {
                return _specializationType ??
                    (_specializationType = (from @interface in typeof(T).GetInterfaces()
                                            where (@interface.IsGenericType) && (typeof(IController<>).IsAssignableFrom(@interface.GetGenericTypeDefinition()))
                                            from type in @interface.GetGenericArguments()
                                            select type).FirstOrDefault() ?? typeof(object));
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

            if (SpecializationType == typeof(object))
            {
                return;
            }

            Uri baseUri = apiDocumentation.Context.BaseUriSelector.SelectBaseUri(new EntityId(new Uri("/", UriKind.Relative)));
            IDictionary<Type, Tuple<Rdfs.IResource, bool>> typeDefinitions = new Dictionary<Type, Tuple<Rdfs.IResource, bool>>();
            var description = _descriptionBuilder.BuildDescriptor();
            bool requiresRdf;
            IClass specializationType = BuildTypeDescription(apiDocumentation, SpecializationType, typeDefinitions, out requiresRdf);
            typeDefinitions[SpecializationType] = new Tuple<Rdfs.IResource, bool>(specializationType, requiresRdf);
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

        private IOperation BuildOperation(IApiDocumentation apiDocumentation, Uri baseUri, OperationInfo<Verb> operation, IDictionary<Type, Tuple<Rdfs.IResource, bool>> typeDefinitions, out IIriTemplate template)
        {
            EntityId methodId = new EntityId(!operation.Arguments.Any() ? operation.Uri.Combine(baseUri) :
                operation.Uri.Combine(baseUri).AddFragment(String.Format(
                "{0}{1}",
                operation.ProtocolSpecificCommand,
                String.Join("And", operation.Arguments.Select(argument => (argument.VariableName ?? argument.Parameter.Name).ToUpperCamelCase())))));
            IOperation result = apiDocumentation.Context.Create<IOperation>(methodId);
            result.Label = operation.UnderlyingMethod.Name;
            result.Description = _xmlDocProvider.GetDescription(operation.UnderlyingMethod);
            result.Method.Add(_descriptionBuilder.GetMethodVerb(operation.UnderlyingMethod).ToString());
            template = BuildTemplate(apiDocumentation, operation, result, typeDefinitions);
            bool isRdfRequired = false;
            bool requiresRdf = false;
            Type type;
            if (((type = operation.UnderlyingMethod.DeclaringType.GetInterfaces()
                .FirstOrDefault(@interface => (@interface.IsGenericType) && (typeof(IWriteController<,>).IsAssignableFrom(@interface.GetGenericTypeDefinition())))) != null) &&
                (operation.UnderlyingMethod.DeclaringType.GetInterfaceMap(type).TargetMethods.Contains(operation.UnderlyingMethod)))
            {
                ParameterInfo[] parameters;
                if ((parameters = operation.UnderlyingMethod.GetParameters()).Length > 1)
                {
                    result.Expects.Add(BuildTypeDescription(apiDocumentation, parameters[1].ParameterType, typeDefinitions, out isRdfRequired));
                    requiresRdf |= isRdfRequired;
                }

                if (parameters[0].IsOut)
                {
                    result.Returns.Add(BuildTypeDescription(apiDocumentation, parameters[0].ParameterType, typeDefinitions, out isRdfRequired));
                }

                requiresRdf |= isRdfRequired;
            }
            else
            {
                foreach (var parameter in operation.Arguments.Where(parameter => parameter.Source is FromBodyAttribute))
                {
                    result.Expects.Add(BuildTypeDescription(apiDocumentation, parameter.Parameter.ParameterType, typeDefinitions, out isRdfRequired));
                    requiresRdf |= isRdfRequired;
                }

                foreach (var output in operation.Results.Where(output => output.Target is ToBodyAttribute))
                {
                    result.Returns.Add(BuildTypeDescription(apiDocumentation, output.Parameter.ParameterType, typeDefinitions, out isRdfRequired));
                    requiresRdf |= isRdfRequired;
                }
            }

            result.MediaTypes.AddRange(BuildOperationMediaType(operation, requiresRdf));
            return result;
        }

        private IEnumerable<string> BuildOperationMediaType(OperationInfo<Verb> operation, bool requiresRdf)
        {
            IList<string> result = operation.UnderlyingMethod.GetCustomAttributes<AsMediaTypeAttribute>().Select(mediaType => mediaType.MediaType).ToList();
            if (result.Count != 0)
            {
                return result;
            }

            result.AddRange(requiresRdf ? EntityConverter.MediaTypes : new[] { JsonConverter.ApplicationJson, XmlConverter.ApplicationXml, XmlConverter.TextXml });
            return result;
        }

        private IIriTemplate BuildTemplate(IApiDocumentation apiDocumentation, OperationInfo<Verb> operation, IOperation operationDocumentation, IDictionary<Type, Tuple<Rdfs.IResource, bool>> typeDefinitions)
        {
            IEnumerable<ArgumentInfo> parameterMapping;
            var uriTemplate = _descriptionBuilder.GetOperationUriTemplate(operation.UnderlyingMethod, out parameterMapping);
            if (String.IsNullOrEmpty(uriTemplate))
            {
                return null;
            }

            IIriTemplate template = null;
            var templateUri = operationDocumentation.Id.Uri.AddFragment("template");
            foreach (var templateMapping in from mapping in parameterMapping 
                                            where !(mapping.Source is FromBodyAttribute)
                                            select BuildTemplateMapping(apiDocumentation, templateUri, mapping, typeDefinitions))
            {
                if (template == null)
                {
                    template = apiDocumentation.Context.Create<IIriTemplate>(templateUri);
                    template.Template = uriTemplate;
                }

                template.Mappings.Add(templateMapping);
            }

            return template;
        }

        private IIriTemplateMapping BuildTemplateMapping(IApiDocumentation apiDocumentation, Uri templateUri, ArgumentInfo mapping, IDictionary<Type, Tuple<Rdfs.IResource, bool>> typeDefinitions)
        {
            IIriTemplateMapping templateMapping = apiDocumentation.Context.Create<IIriTemplateMapping>(templateUri.AddFragment(mapping.VariableName));
            templateMapping.Variable = mapping.VariableName;
            templateMapping.Required = mapping.Parameter.ParameterType.IsValueType;
            var linqBehavior = mapping.Parameter.GetCustomAttribute<LinqServerBehaviorAttribute>(true);
            if (linqBehavior != null)
            {
                templateMapping.Property = apiDocumentation.Context.Create<Rdfs.IProperty>(
                    DescriptionController<IController>.VocabularyBaseUri.AbsoluteUri + linqBehavior.Operation.ToString().ToLowerCamelCase());
                Tuple<Rdfs.IResource, bool> range;
                if (!typeDefinitions.TryGetValue(typeof(int), out range))
                {
                    range = new Tuple<Rdfs.IResource, bool>(BuildTypeDescription(apiDocumentation, typeof(int), typeDefinitions), false);
                }

                templateMapping.Property.Range.Add(range.Item1);
            }
            else if (SpecializationType != typeof(object))
            {
                templateMapping.Property = GetMappingProperty(apiDocumentation, mapping.Parameter, SpecializationType, typeDefinitions);
            }

            return templateMapping;
        }

        private Rdfs.IProperty GetMappingProperty(IApiDocumentation apiDocumentation, ParameterInfo parameter, Type type, IDictionary<Type, Tuple<Rdfs.IResource, bool>> typeDefinitions)
        {
            Tuple<Rdfs.IResource, bool> classDescription;
            Rdfs.IResource @class;
            @class = (!typeDefinitions.TryGetValue(type, out classDescription) ? BuildTypeDescription(apiDocumentation, type, typeDefinitions) : classDescription.Item1);
            if (@class is IClass)
            {
                return (from supportedProperty in ((IClass)@class).SupportedProperties
                        let property = supportedProperty.Property
                        where StringComparer.OrdinalIgnoreCase.Equals(property.Label, parameter.Name)
                        select property).FirstOrDefault();
            }

            return null;
        }

        private IClass BuildTypeDescription(IApiDocumentation apiDocumentation, Type type, IDictionary<Type, Tuple<Rdfs.IResource, bool>> typeDefinitions)
        {
            bool requiresRdf;
            return BuildTypeDescription(apiDocumentation, type, typeDefinitions, out requiresRdf);
        }

        private IClass BuildTypeDescription(IApiDocumentation apiDocumentation, Type type, IDictionary<Type, Tuple<Rdfs.IResource, bool>> typeDefinitions, out bool requiresRdf)
        {
            Uri uri;
            IClass result;
            requiresRdf = false;
            Type itemType = type.FindItemType();
            if (TypeDescriptions.TryGetValue(itemType, out uri))
            {
                return BuildDatatypeDescription(apiDocumentation, uri);
            }

            if (type.IsList())
            {
                result = CreateListDefinition(apiDocumentation, type, typeDefinitions, out requiresRdf, type.IsGenericList());
                typeDefinitions[type] = new Tuple<Rdfs.IResource, bool>(result, requiresRdf);
                return result;
            }

            var classUri = new Uri(String.Format("urn:" + DotNetSymbol + ":{0}", itemType.FullName.Replace("&", String.Empty)));
            if (typeof(IEntity).IsAssignableFrom(itemType))
            {
                classUri = apiDocumentation.Context.Mappings.MappingFor(itemType).Classes.Select(item => item.Uri).FirstOrDefault() ?? classUri;
            }

            result = apiDocumentation.Context.Create<IClass>(classUri);
            result.Label = type.Name.Replace("&", String.Empty);
            result.Description = _xmlDocProvider.GetDescription(itemType);
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                result.SupportedProperties.Add(BuildSupportedProperty(apiDocumentation, result, itemType, property, typeDefinitions));
            }

            typeDefinitions[type] = new Tuple<Rdfs.IResource, bool>(result, requiresRdf);
            return result;
        }

        private IClass BuildDatatypeDescription(IApiDocumentation apiDocumentation, Uri uri)
        {
            var datatype = apiDocumentation.Context.Create<IResource>(new EntityId(uri));
            var definition = apiDocumentation.Context.Create<IDatatypeDefinition>(new EntityId(String.Format("urn:guid:{0}", Guid.NewGuid())));
            definition.Datatype = datatype;
            return definition.AsEntity<IClass>();
        }

        private ISupportedProperty BuildSupportedProperty(IApiDocumentation apiDocumentation, IClass @class, Type ownerType, PropertyInfo property, IDictionary<Type, Tuple<Rdfs.IResource, bool>> typeDefinitions)
        {
            var propertyUri = (!typeof(IEntity).IsAssignableFrom(ownerType) ? @class.Id.Uri.AddName(property.Name) :
                apiDocumentation.Context.Mappings.MappingFor(ownerType).PropertyFor(property.Name).Uri);
            var result = apiDocumentation.Context.Create<ISupportedProperty>(new Uri(String.Format("urn:" + HydraSymbol + ":{0}.{1}", ownerType, property.Name)));
            result.ReadOnly = (!property.PropertyType.IsCollection()) && (!property.CanWrite);
            result.WriteOnly = !property.CanRead;
            result.Required = (property.PropertyType.IsValueType) || (property.GetCustomAttribute<RequiredAttribute>() != null);
            result.Property = (property.GetCustomAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>() != null ?
                apiDocumentation.Context.Create<Owl.IInverseFunctionalProperty>(propertyUri) :
                apiDocumentation.Context.Create<Rdfs.IProperty>(propertyUri));
            result.Property.Label = property.Name;
            result.Property.Description = _xmlDocProvider.GetDescription(property);
            result.Property.Domain.Add(@class);
            Tuple<Rdfs.IResource, bool> range;
            if (!typeDefinitions.TryGetValue(property.PropertyType, out range))
            {
                bool requiresRdf;
                var propertyType = BuildTypeDescription(apiDocumentation, property.PropertyType, typeDefinitions, out requiresRdf);
                typeDefinitions[property.PropertyType] = range = new Tuple<Rdfs.IResource, bool>(propertyType, requiresRdf);
            }

            result.Property.Range.Add(range.Item1);
            if ((System.Reflection.TypeExtensions.IsEnumerable(property.PropertyType)) && (property.PropertyType != typeof(byte[])))
            {
                return result;
            }

            var restriction = apiDocumentation.Context.Create<IRestriction>(@class.CreateBlankId());
            restriction.OnProperty = result.Property;
            restriction.MaxCardinality = 1u;
            @class.SubClassOf.Add(restriction);
            return result;
        }

        private IClass CreateListDefinition(IApiDocumentation apiDocumentation, Type type, IDictionary<Type, Tuple<Rdfs.IResource, bool>> typeDefinitions, out bool requiresRdf, bool isGeneric = true)
        {
            requiresRdf = false;
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
            typeConstrain.AllValuesFrom = (typeDefinitions.ContainsKey(itemType) ? typeDefinitions[itemType].Item1 : 
                BuildTypeDescription(apiDocumentation, itemType, typeDefinitions, out requiresRdf));
            result.SubClassOf.Add(typeConstrain);
            requiresRdf = typeDefinitions[itemType].Item2;

            var restConstrain = apiDocumentation.Context.Create<Owl.IRestriction>(result.CreateBlankId());
            restConstrain.OnProperty = apiDocumentation.Context.Create<Rdfs.IProperty>(new EntityId(Rdf.rest));
            restConstrain.AllValuesFrom = result;
            result.SubClassOf.Add(restConstrain);
            return result;
        }
    }
}