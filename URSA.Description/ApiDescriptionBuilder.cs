using RomanticWeb.Entities;
using RomanticWeb.Model;
using RomanticWeb.Vocabularies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Mapping;

namespace URSA.Web.Http.Description
{
    /// <summary>Builds API description.</summary>
    /// <typeparam name="T">Type of the API to describe</typeparam>
    public class ApiDescriptionBuilder<T> where T : IController
    {
        private const string Xsd = "http://www.w3.org/2001/XMLSchema#";
        private const string OGuid = "http://openguid.net/rdf#";
        private static readonly IDictionary<Type, Uri> TypeDescriptions = new Dictionary<Type, Uri>
        {
            { typeof(byte[]), new Uri(Xsd + "hexBinary") },
            { typeof(string), new Uri(Xsd + "string") },
            { typeof(bool), new Uri(Xsd + "boolean") },
            { typeof(sbyte), new Uri(Xsd + "byte") },
            { typeof(byte), new Uri(Xsd + "unsignedByte") },
            { typeof(short), new Uri(Xsd + "short") },
            { typeof(ushort), new Uri(Xsd + "unsignedShort") },
            { typeof(int), new Uri(Xsd + "int") },
            { typeof(uint), new Uri(Xsd + "unsignedInt") },
            { typeof(long), new Uri(Xsd + "long") },
            { typeof(ulong), new Uri(Xsd + "unsignedLong") },
            { typeof(float), new Uri(Xsd + "float") },
            { typeof(double), new Uri(Xsd + "double") },
            { typeof(decimal), new Uri(Xsd + "decimal") },
            { typeof(DateTime), new Uri(Xsd + "dateTime") },
            { typeof(TimeSpan), new Uri(Xsd + "duration") },
            { typeof(Uri), new Uri(Xsd + "anyUri") },
            { typeof(Guid), new Uri(OGuid + "guid") }
        };

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
            IDictionary<Type, IEntity> typeDefinitions = new Dictionary<Type, IEntity>();
            var description = _descriptionBuilder.BuildDescriptor();
            if (SpecializationType != typeof(object))
            {
                IClass specializationType = (IClass)(typeDefinitions[SpecializationType] = BuildTypeDescription(apiDocumentation, SpecializationType, typeDefinitions));
                apiDocumentation.SupportedClasses.Add(specializationType);
                foreach (Web.Description.Http.OperationInfo operation in description.Operations)
                {
                    IIriTemplate template;
                    var operationDefinition = BuildOperation(apiDocumentation, baseUri, operation, typeDefinitions, out template);
                    if (template != null)
                    {
                        apiDocumentation.Context.Store.ReplacePredicateValues(
                            specializationType.Id,
                            Node.ForUri(template.Id.Uri),
                            () => new Node[] { Node.ForUri(operationDefinition.Id.Uri) },
                            specializationType.Id.Uri);
                    }
                    else
                    {
                        specializationType.SupportedOperations.Add(operationDefinition);
                    }
                }
            }
        }

        private IOperation BuildOperation(IApiDocumentation apiDocumentation, Uri baseUri, Web.Description.Http.OperationInfo operation, IDictionary<Type, IEntity> typeDefinitions, out IIriTemplate template)
        {
            var nonBodyArguments = operation.Arguments.Where(argument => (argument.Source is FromQueryStringAttribute) || (argument.Source is FromUriAttribute)).ToList();
            EntityId methodId = new EntityId(nonBodyArguments.Count == 0 ? operation.Uri.Combine(baseUri) :
                operation.Uri.Combine(baseUri).AddFragment(
                    String.Format("with{0}", String.Join("And", nonBodyArguments.Select(argument => argument.VariableName.ToUpperCamelCase())))));
            IOperation result = apiDocumentation.Context.Create<IOperation>(methodId);
            result.Label = operation.UnderlyingMethod.Name;
            result.Method.Add(_descriptionBuilder.GetMethodVerb(operation.UnderlyingMethod).ToString());
            if ((template = BuildTemplate(apiDocumentation, operation, result, typeDefinitions)) == null)
            {
                foreach (var parameter in operation.Arguments)
                {
                    if (parameter.Source is FromBodyAttribute)
                    {
                        result.Expects.Add(BuildBodyArgument(apiDocumentation, parameter.Parameter, typeDefinitions));
                    }
                }
            }

            return result;
        }

        private IIriTemplate BuildTemplate(IApiDocumentation apiDocumentation, Web.Description.Http.OperationInfo operation, IOperation operationDocumentation, IDictionary<Type, IEntity> typeDefinitions)
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

                    if (!(mapping.Source is FromBodyAttribute))
                    {
                        IIriTemplateMapping templateMapping = apiDocumentation.Context.Create<IIriTemplateMapping>(template.Id.Uri.AddFragment(mapping.VariableName));
                        templateMapping.Variable = mapping.VariableName;
                        templateMapping.Required = mapping.Parameter.ParameterType.IsValueType;
                        if (SpecializationType != typeof(object))
                        {
                            templateMapping.Property = GetMappingProperty(apiDocumentation, mapping.Parameter, SpecializationType, typeDefinitions);
                        }

                        template.Mappings.Add(templateMapping);
                    }
                    else
                    {
                        operationDocumentation.Expects.Add(BuildBodyArgument(apiDocumentation, mapping.Parameter, typeDefinitions));
                    }
                }
            }

            return template;
        }

        private IClass BuildBodyArgument(IApiDocumentation apiDocumentation, ParameterInfo parameter, IDictionary<Type, IEntity> typeDefinitions)
        {
            var expected = BuildTypeDescription(apiDocumentation, parameter.ParameterType, typeDefinitions);
            if (!(expected is IClass))
            {
                var definition = apiDocumentation.Context.Create<IDatatypeDefinition>(new EntityId(new Uri("urn:ursa:" + parameter.ParameterType)));
                definition.Datatype = expected;
                expected = definition;
            }

            return (IClass)expected;
        }

        private Rdfs.IProperty GetMappingProperty(IApiDocumentation apiDocumentation, ParameterInfo parameter, Type type, IDictionary<Type, IEntity> typeDefinitions)
        {
            IEntity @class;
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

        private IEntity BuildTypeDescription(IApiDocumentation apiDocumentation, Type type, IDictionary<Type, IEntity> typeDefinitions)
        {
            Uri uri;
            if (TypeDescriptions.TryGetValue(type, out uri))
            {
                return apiDocumentation.Context.Create<IEntity>(new EntityId(uri));
            }

            IClass result = null;
            if (type.IsGenericList())
            {
                return typeDefinitions[type] = CreateGenericListDefinition(apiDocumentation, type, typeDefinitions);
            }
            else if (type.IsGenericCollection())
            {
                return typeDefinitions[type] = CreateGenericCollectionDefinition(apiDocumentation, type, typeDefinitions);
            }
            else
            {
                result = apiDocumentation.Context.Create<IClass>(new EntityId(new Uri(String.Format("urn:net:{0}", type.FullName))));
                result.Label = type.Name;
                foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var supportedProperty = apiDocumentation.Context.Create<ISupportedProperty>(new Uri(String.Format("urn:hydra:{0}.{1}", type.FullName, property.Name)));
                    supportedProperty.ReadOnly = !property.CanWrite;
                    supportedProperty.WriteOnly = !property.CanRead;
                    supportedProperty.Required = property.PropertyType.IsValueType;
                    supportedProperty.Property = apiDocumentation.Context.Create<Rdfs.IProperty>(result.Id.Uri.AddName(property.Name));
                    supportedProperty.Property.Label = property.Name;
                    supportedProperty.Property.Domain.Add(result);
                    IEntity range;
                    if (!typeDefinitions.TryGetValue(property.PropertyType, out range))
                    {
                        typeDefinitions[property.PropertyType] = range = BuildTypeDescription(apiDocumentation, property.PropertyType, typeDefinitions);
                    }

                    supportedProperty.Property.Range.Add(range);
                    result.SupportedProperties.Add(supportedProperty);
                }

                return typeDefinitions[type] = result;
            }
        }

        private IClass CreateGenericListDefinition(IApiDocumentation apiDocumentation, Type type, IDictionary<Type, IEntity> typeDefinitions)
        {
            Type itemType = type.GetItemType();
            IClass result = apiDocumentation.Context.Create<IClass>(new EntityId(new Uri(String.Format("urn:net:list:{0}", itemType.FullName))));
            result.Label = type.Name;
            result.SubClassOf.Add(apiDocumentation.Context.Create<IClass>(new EntityId(Rdf.List)));

            Owl.IRestriction typeConstrain = apiDocumentation.Context.Create<Owl.IRestriction>(result.CreateBlankId());
            typeConstrain.OnProperty = apiDocumentation.Context.Create<Rdfs.IProperty>(new EntityId(Rdf.first));
            typeConstrain.AllValuesFrom = typeDefinitions.GetOrCreate(itemType, () => BuildTypeDescription(apiDocumentation, itemType, typeDefinitions));
            result.SubClassOf.Add(typeConstrain);

            Owl.IRestriction restConstrain = apiDocumentation.Context.Create<Owl.IRestriction>(result.CreateBlankId());
            restConstrain.OnProperty = apiDocumentation.Context.Create<Rdfs.IProperty>(new EntityId(Rdf.rest));
            restConstrain.AllValuesFrom = result;
            result.SubClassOf.Add(restConstrain);
            return result;
        }

        private IClass CreateGenericCollectionDefinition(IApiDocumentation apiDocumentation, Type type, IDictionary<Type, IEntity> typeDefinitions)
        {
            Type itemType = type.GetItemType();
            IClass result = apiDocumentation.Context.Create<IClass>(new EntityId(new Uri(String.Format("urn:net:collection:{0}", itemType.FullName))));
            result.Label = type.Name;
            result.SubClassOf.Add(apiDocumentation.Context.Create<IClass>(new EntityId(Rdf.Bag)));

            Owl.IRestriction typeConstrain = apiDocumentation.Context.Create<Owl.IRestriction>(result.CreateBlankId());
            typeConstrain.OnProperty = apiDocumentation.Context.Create<Rdfs.IProperty>(new EntityId(new Uri(Rdf.BaseUri + "_1")));
            typeConstrain.AllValuesFrom = typeDefinitions.GetOrCreate(itemType, () => BuildTypeDescription(apiDocumentation, itemType, typeDefinitions));
            result.SubClassOf.Add(typeConstrain);
            return result;
        }
    }
}