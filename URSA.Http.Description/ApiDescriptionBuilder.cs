using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using RDeF.Entities;
using RDeF.Mapping.Entities;
using RollerCaster;
using URSA.Reflection;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Configuration;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Mapping;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;
using IClass = URSA.Web.Http.Description.Hydra.IClass;

namespace URSA.Web.Http.Description
{
    /// <summary>Builds API description.</summary>
    public abstract class ApiDescriptionBuilder : IApiDescriptionBuilder
    {
        internal static readonly string[] RdfMediaTypes = EntityConverter.MediaTypes;
        internal static readonly string[] NonRdfMediaTypes = { JsonConverter.ApplicationJson, XmlConverter.ApplicationXml, XmlConverter.TextXml };

        private readonly IHttpServerConfiguration _httpServerConfiguration;
        private readonly IHttpControllerDescriptionBuilder _descriptionBuilder;
        private readonly IXmlDocProvider _xmlDocProvider;
        private readonly IEnumerable<ITypeDescriptionBuilder> _typeDescriptionBuilders;
        private readonly IEnumerable<IServerBehaviorAttributeVisitor> _serverBehaviorAttributeVisitors;

        /// <summary>Initializes a new instance of the <see cref="ApiDescriptionBuilder" /> class.</summary>
        /// <param name="httpServerConfiguration">HTTP server configuration with base Uri.</param>
        /// <param name="descriptionBuilder">Description builder.</param>
        /// <param name="xmlDocProvider">The XML documentation provider.</param>
        /// <param name="typeDescriptionBuilders">Type description builders.</param>
        /// <param name="serverBehaviorAttributeVisitors">Server behavior attribute visitors.</param>
        protected ApiDescriptionBuilder(
            IHttpServerConfiguration httpServerConfiguration,
            IHttpControllerDescriptionBuilder descriptionBuilder,
            IXmlDocProvider xmlDocProvider,
            IEnumerable<ITypeDescriptionBuilder> typeDescriptionBuilders,
            IEnumerable<IServerBehaviorAttributeVisitor> serverBehaviorAttributeVisitors)
        {
            if (httpServerConfiguration == null)
            {
                throw new ArgumentNullException("httpServerConfiguration");
            }

            if (descriptionBuilder == null)
            {
                throw new ArgumentNullException("descriptionBuilder");
            }

            if (xmlDocProvider == null)
            {
                throw new ArgumentNullException("xmlDocProvider");
            }

            if (typeDescriptionBuilders == null)
            {
                throw new ArgumentNullException("typeDescriptionBuilders");
            }

            if (!typeDescriptionBuilders.Any())
            {
                throw new ArgumentOutOfRangeException("typeDescriptionBuilders");
            }

            _httpServerConfiguration = httpServerConfiguration;
            _descriptionBuilder = descriptionBuilder;
            _xmlDocProvider = xmlDocProvider;
            _typeDescriptionBuilders = typeDescriptionBuilders;
            _serverBehaviorAttributeVisitors = serverBehaviorAttributeVisitors ?? new IServerBehaviorAttributeVisitor[0];
        }

        /// <inheritdoc />
        public abstract Type SpecializationType { get; }

        /// <inheritdoc />
        public void BuildDescription(IApiDocumentation apiDocumentation, IEnumerable<Uri> profiles)
        {
            if (apiDocumentation == null)
            {
                throw new ArgumentNullException("apiDocumentation");
            }

            if (SpecializationType == typeof(object))
            {
                return;
            }

            var typeDescriptionBuilder = GetTypeDescriptionBuilder(profiles);
            var descriptionContext = DescriptionContext.ForType(apiDocumentation, SpecializationType, typeDescriptionBuilder);
            IClass specializationType = descriptionContext.BuildTypeDescription();
            BuildDescription(descriptionContext, specializationType);
        }

        /// <inheritdoc />
        public void BuildOperationDescription(IEntity context, OperationInfo<Verb> operationInfo, IEnumerable<Uri> profiles)
        {
            var typeDescriptionBuilder = GetTypeDescriptionBuilder(profiles);
            var descriptionContext = DescriptionContext.ForType(context, SpecializationType, typeDescriptionBuilder);
            IClass specializationType = descriptionContext.BuildTypeDescription();
            BuildOperationDescription(descriptionContext, operationInfo, specializationType);
        }

        internal IResource DetermineOperationOwner(OperationInfo<Verb> operation, DescriptionContext context, IClass specializationType)
        {
            IResource result = specializationType;
            PropertyInfo matchingProperty;
            if ((!typeof(IReadController<,>).IsAssignableFromSpecificGeneric(_descriptionBuilder.BuildDescriptor().ControllerType)) ||
                ((matchingProperty = operation.UnderlyingMethod.MatchesPropertyOf(context.Type, typeof(IControlledEntity<>).GetProperties().First().Name)) == null))
            {
                return result;
            }

            var propertyId = context.TypeDescriptionBuilder.GetSupportedPropertyId(matchingProperty, context.Type);
            return context.Entity.Context.Load<ISupportedProperty>(propertyId);
        }

        private static void BuildOperationMediaType(IOperation result, bool requiresRdf)
        {
            foreach (var mediaType in (requiresRdf ? RdfMediaTypes : NonRdfMediaTypes))
            {
                result.MediaTypes.Add(mediaType);
            }
        }

        private static void BuildOperationMediaType(IOperation result, OperationInfo operation, bool requiresRdf)
        {
            foreach (var mediaType in operation.UnderlyingMethod.GetCustomAttributes<AsMediaTypeAttribute>())
            {
                result.MediaTypes.Add(mediaType.MediaType);
            }

            BuildOperationMediaType(result, requiresRdf);
        }

        private static Rdfs.IProperty GetMappingProperty(DescriptionContext context, ParameterInfo parameter)
        {
            IClass description = context[context.Type];
            Rdfs.IProperty resultCandidate = null;
            IResource parameterType = null;
            foreach (var supportedProperty in description.SupportedProperties)
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(supportedProperty.Property.Label, parameter.Name))
                {
                    return supportedProperty.Property;
                }

                if (parameterType == null)
                {
                    parameterType = (context.ContainsType(parameter.ParameterType) ? context[parameter.ParameterType] : context.BuildTypeDescription());
                }

                if (supportedProperty.Property.Range.Any(range => (range.Iri == parameterType.Iri) ||
                    ((range is IClass) && (((IClass)range).SubClassOf.Any(subClass => subClass.Iri == parameterType.Iri)))))
                {
                    resultCandidate = supportedProperty.Property;
                }
            }

            return resultCandidate;
        }

        private ITypeDescriptionBuilder GetTypeDescriptionBuilder(IEnumerable<Uri> profiles)
        {
            if ((profiles == null) || (!profiles.Any()))
            {
                return _typeDescriptionBuilders.FirstOrDefault();
            }

            var result = (from descriptionBuilder in _typeDescriptionBuilders
                          from supportedProfile in descriptionBuilder.SupportedProfiles
                          join requestedProfile in profiles on supportedProfile equals requestedProfile into matchingProfiles
                          orderby matchingProfiles.Count() descending
                          select descriptionBuilder).FirstOrDefault();

            if (result == null)
            {
                throw new InvalidOperationException(String.Format("No matching profile ({0}) type description builder found.", String.Join("', '", profiles)));
            }

            return result;
        }

        private void BuildDescription(DescriptionContext context, IClass specializationType)
        {
            if (context.Entity.Is(context.Entity.Context.Mappings.FindEntityMappingFor<IApiDocumentation>().Classes.Select(@class => @class.Term)))
            {
                context.Entity.ActLike<IApiDocumentation>().SupportedClasses.Add(specializationType);
            }

            var description = _descriptionBuilder.BuildDescriptor();
            foreach (OperationInfo<Verb> operation in description.Operations)
            {
                BuildOperationDescription(context, operation, specializationType);
            }
        }

        private void BuildOperationDescription(DescriptionContext context, OperationInfo<Verb> operation, IClass specializationType)
        {
            IIriTemplate template;
            var operationDefinition = BuildOperation(context, operation, out template);
            IResource operationOwner = DetermineOperationOwner(operation, context, specializationType);
            if (template != null)
            {
                ITemplatedLink templatedLink = context.Entity.Context.Create<ITemplatedLink>(new Iri(((Uri)template.Iri).AbsoluteUri.Replace("#template", "#withTemplate")));
                templatedLink.SupportedOperations.Add(operationDefinition);
                var templatedOperation = context.Entity.Context.Load<ITemplatedOperation>(
                    operationOwner.Iri,
                    entity => entity.WithProperty(instance => instance.TemplatedLink).MappedTo(templatedLink.Iri).WithDefaultConverter());
                templatedOperation.TemplatedLink = context.Entity.Context.Load<IEntity>(template.Iri);
            }
            else
            {
                (operationOwner is ISupportedOperationsOwner ? ((ISupportedOperationsOwner)operationOwner).SupportedOperations : operationOwner.Operations).Add(operationDefinition);
            }
        }

        private IOperation BuildOperation(DescriptionContext context, OperationInfo<Verb> operation, out IIriTemplate template)
        {
            IOperation result = operation.AsOperation(_httpServerConfiguration.BaseUri, context.Entity);
            result.Label = operation.UnderlyingMethod.Name;
            result.Description = _xmlDocProvider.GetDescription(operation.UnderlyingMethod);
            result.Method.Add(operation.ProtocolSpecificCommand.ToString());
            template = BuildTemplate(context, operation, result);
            bool requiresRdf = context.RequiresRdf(SpecializationType);
            bool isRdfRequired = requiresRdf;
            var arguments = operation.Arguments.Where(parameter => parameter.Source is FromBodyAttribute).Select(parameter => parameter.Parameter);
            var results = operation.Results.Where(output => output.Target is ToBodyAttribute).Select(parameter => parameter.Parameter);

            foreach (var value in arguments)
            {
                var expected = (context.ContainsType(value.ParameterType) ? context[value.ParameterType] : context.ForType(value.ParameterType).BuildTypeDescription(out isRdfRequired));
                expected = context.SubClass(expected, value.ParameterType);
                expected.Label = value.Name ?? "instance";
                expected.Description = _xmlDocProvider.GetDescription(operation.UnderlyingMethod, value);
                result.Expects.Add(expected);
                requiresRdf |= isRdfRequired;
            }

            foreach (var value in results)
            {
                var returned = (context.ContainsType(value.ParameterType) ? context[value.ParameterType] : context.ForType(value.ParameterType).BuildTypeDescription(out isRdfRequired));
                returned = context.SubClass(returned, value.ParameterType);
                returned.Description = _xmlDocProvider.GetDescription(operation.UnderlyingMethod, value);
                result.Returns.Add(returned);
                requiresRdf |= isRdfRequired;
            }

            BuildStatusCodes(result, operation);
            BuildOperationMediaType(result, operation, requiresRdf);
            if (!result.MediaTypes.Any())
            {
                BuildOperationMediaType(result, requiresRdf);
            }

            return result;
        }

        private void BuildStatusCodes(IOperation result, OperationInfo<Verb> operation)
        {
            result.StatusCodes.AddRange(
                operation.UnderlyingMethod.DiscoverCrudStatusCodeNumbers(operation.ProtocolSpecificCommand, operation.Controller.ControllerType)
                .Union(
                from exceptionTypeName in _xmlDocProvider.GetExceptions(operation.UnderlyingMethod)
                select (int)exceptionTypeName.ToHttpStatusCode())
                .Union(
                from claimType in operation.UnifiedSecurityRequirements.Denied
                select (int)(claimType == ClaimTypes.Anonymous ? HttpStatusCode.Unauthorized : HttpStatusCode.Forbidden))
                .Union(
                from claimType in operation.UnifiedSecurityRequirements.Allowed
                where claimType != ClaimTypes.Anonymous
                select (int)HttpStatusCode.Forbidden)
                .Distinct());
        }

        private IIriTemplate BuildTemplate(DescriptionContext context, OperationInfo<Verb> operation, IOperation operationDocumentation)
        {
            IEnumerable<ArgumentInfo> parameterMapping;
            var uriTemplate = _descriptionBuilder.GetOperationUrlTemplate(operation.UnderlyingMethod, out parameterMapping);
            if (String.IsNullOrEmpty(uriTemplate))
            {
                return null;
            }

            IIriTemplate template = null;
            var templateUri = ((Uri)operationDocumentation.Iri).AddFragment("template");
            var templateMappings = from mapping in parameterMapping where !(mapping.Source is FromBodyAttribute) select BuildTemplateMapping(context, templateUri, operation, mapping);
            foreach (var templateMapping in templateMappings)
            {
                if (template == null)
                {
                    template = context.Entity.Context.Create<IIriTemplate>(templateUri);
                    template.Template = uriTemplate;
                }

                template.Mappings.Add(templateMapping);
            }

            return template;
        }

        private IIriTemplateMapping BuildTemplateMapping(DescriptionContext context, Uri templateUri, OperationInfo<Verb> operation, ArgumentInfo mapping)
        {
            IIriTemplateMapping templateMapping = context.Entity.Context.Create<IIriTemplateMapping>(templateUri.AddFragment(mapping.VariableName));
            templateMapping.Variable = mapping.VariableName;
            templateMapping.Required = (mapping.Parameter.ParameterType.GetTypeInfo().IsValueType) && (!mapping.Parameter.HasDefaultValue);
            templateMapping.Description = _xmlDocProvider.GetDescription(operation.UnderlyingMethod, mapping.Parameter);
            var linqBehaviors = mapping.Parameter.GetCustomAttributes<LinqServerBehaviorAttribute>(true);
            if (linqBehaviors.Any())
            {
                foreach (var visitor in _serverBehaviorAttributeVisitors)
                {
                    linqBehaviors.Accept(mapping.Parameter.ParameterType, visitor, templateMapping, context);
                }
            }
            else if (context.Type != typeof(object))
            {
                templateMapping.Property = GetMappingProperty(context, mapping.Parameter);
            }

            return templateMapping;
        }
    }

    /// <summary>Builds API description.</summary>
    /// <typeparam name="T">Type of the API to describe</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Suppression is OK - generic and non-generic class.")]
    public class ApiDescriptionBuilder<T> : ApiDescriptionBuilder, IApiDescriptionBuilder<T> where T : IController
    {
        private Type _specializationType;

        /// <summary>Initializes a new instance of the <see cref="ApiDescriptionBuilder{T}" /> class.</summary>
        /// <param name="httpServerConfiguration">HTTP server configuration with base Uri.</param>
        /// <param name="descriptionBuilder">Description builder.</param>
        /// <param name="xmlDocProvider">The XML documentation provider.</param>
        /// <param name="typeDescriptionBuilders">Type description builders.</param>
        /// <param name="serverBehaviorAttributeVisitors">Server behavior attribute visitors.</param>
        public ApiDescriptionBuilder(
            IHttpServerConfiguration httpServerConfiguration,
            IHttpControllerDescriptionBuilder<T> descriptionBuilder,
            IXmlDocProvider xmlDocProvider,
            IEnumerable<ITypeDescriptionBuilder> typeDescriptionBuilders,
            IEnumerable<IServerBehaviorAttributeVisitor> serverBehaviorAttributeVisitors)
            : base(httpServerConfiguration, descriptionBuilder, xmlDocProvider, typeDescriptionBuilders, serverBehaviorAttributeVisitors)
        {
        }

        /// <inheritdoc />
        public override Type SpecializationType
        {
            get
            {
                return _specializationType ?? (_specializationType =
                    (_specializationType = typeof(T).GetTypeInfo().GetImplementationOfAny(typeof(IController<>), typeof(IAsyncController<>))) != null ?
                    _specializationType.GetGenericArguments()[0] :
                    typeof(object));
            }
        }
    }
}