using RomanticWeb.Entities;
using RomanticWeb.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using URSA.Web.Description;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description
{
    /// <summary>Builds API description.</summary>
    /// <typeparam name="T">Type of the API to describe</typeparam>
    public class ApiDescriptionBuilder<T> where T : IController
    {
        private IHttpDelegateMapper _handlerMapper;

        /// <summary>Initializes a new instance of the <see cref="ApiDescriptionBuilder{T}" /> class.</summary>
        /// <param name="handlerMapper">Handler mapper instance.</param>
        public ApiDescriptionBuilder(IHttpDelegateMapper handlerMapper)
        {
            if (handlerMapper == null)
            {
                throw new ArgumentNullException("handlerMapper");
            }

            _handlerMapper = handlerMapper;
        }

        /// <summary>Builds an API description.</summary>
        /// <param name="apiDocumentation">API documentation.</param>
        public void BuildDescription(IApiDocumentation apiDocumentation)
        {
            Uri baseUri = apiDocumentation.Context.BaseUriSelector.SelectBaseUri(new EntityId(new Uri("/", UriKind.Relative)));
            IDictionary<Type, IEntity> typeDefinitions = new Dictionary<Type, IEntity>();
            var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method => method.DeclaringType != typeof(object)).Except(
                typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .SelectMany(property => new MethodInfo[] { property.GetGetMethod(), property.GetSetMethod() })
                .Where(method => method != null));
            foreach (var method in methods)
            {
                var methodId = new EntityId(baseUri.AddFragment(method.Name.ToLowerCamelCase()));
                IResource resource = apiDocumentation.Context.Create<IResource>(methodId);
                apiDocumentation.EntryPoints.Add(resource);
                BuildOperation(apiDocumentation, resource, method, typeDefinitions);
            }

            foreach (var typeDefinition in typeDefinitions)
            {
                if (typeDefinition.Value is IClass)
                {
                    apiDocumentation.SupportedClasses.Add((IClass)typeDefinition.Value);
                }
            }
        }

        private void BuildOperation(IApiDocumentation apiDocumentation, IResource resource, MethodInfo method, IDictionary<Type, IEntity> typeDefinitions)
        {
            IOperation operation = apiDocumentation.Context.Create<IOperation>(GenerateIdentifier());
            operation.Method.Add(_handlerMapper.GetMethodVerb(method).ToString());
            IIriTemplate template = BuildTemplate(apiDocumentation, method, typeDefinitions);
            if (template != null)
            {
                apiDocumentation.Context.Store.ReplacePredicateValues(
                    resource.Id,
                    Node.ForUri(template.Id.Uri),
                    () => new Node[] { Node.ForUri(operation.Id.Uri) },
                    null);
            }
            else
            {
                resource.Operations.Add(operation);
            }
        }

        private IIriTemplate BuildTemplate(IApiDocumentation apiDocumentation, MethodInfo method, IDictionary<Type, IEntity> typeDefinitions)
        {
            IIriTemplate template = null;
            IEnumerable<ArgumentInfo> parameterMapping;
            var uriTemplate = _handlerMapper.GetMethodUriTemplate(method, out parameterMapping);
            if ((!String.IsNullOrEmpty(uriTemplate)) && (parameterMapping.Any()))
            {
                template = apiDocumentation.Context.Create<IIriTemplate>(GenerateIdentifier());
                template.Template = uriTemplate;
                foreach (var mapping in parameterMapping)
                {
                    IIriTemplateMapping templateMapping = apiDocumentation.Context.Create<IIriTemplateMapping>(GenerateIdentifier());
                    templateMapping.Variable = mapping.VariableName;
                    templateMapping.Required = mapping.Parameter.ParameterType.IsValueType;
                    var type = GetSpecializationType(method);
                    if (type != null)
                    {
                        templateMapping.Property = GetMappingProperty(apiDocumentation, mapping.Parameter, type, typeDefinitions);
                    }

                    template.Mappings.Add(templateMapping);
                }
            }

            return template;
        }

        private IProperty GetMappingProperty(IApiDocumentation apiDocumentation, ParameterInfo parameter, Type type, IDictionary<Type, IEntity> typeDefinitions)
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
            IClass result = apiDocumentation.Context.Create<IClass>(new EntityId(new Uri("res://" + type.FullName)));
            typeDefinitions[type] = result;
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var supportedProperty = apiDocumentation.Context.Create<ISupportedProperty>(GenerateIdentifier());
                supportedProperty.ReadOnly = !property.CanWrite;
                supportedProperty.WriteOnly = !property.CanRead;
                supportedProperty.Required = property.PropertyType.IsValueType;
                supportedProperty.Property = apiDocumentation.Context.Create<IProperty>(new Uri("res://" + type.FullName + "." + property.Name));
                supportedProperty.Property.Label = property.Name;
                supportedProperty.Property.Domain.Add(result);
                IEntity range;
                if (!typeDefinitions.TryGetValue(property.PropertyType, out range))
                {
                    typeDefinitions[property.PropertyType] = range = BuildTypeDescription(apiDocumentation, property.PropertyType, typeDefinitions);
                }

                supportedProperty.Property.Range.Add(range);
            }

            return result;
        }

        private Type GetSpecializationType(MethodInfo method)
        {
            return (from @interface in method.DeclaringType.GetInterfaces()
                    where typeof(IController<T>).IsAssignableFrom(@interface)
                    from type in @interface.GetGenericArguments()
                    select type).FirstOrDefault();
        }

        private EntityId GenerateIdentifier()
        {
            return new EntityId(String.Format("urn:uuid:{0}", Guid.NewGuid()));
        }
    }
}