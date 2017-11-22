using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using RDeF.Entities;
using Tavis.UriTemplates;
using URSA.Reflection;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Configuration;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides an API entry point description.</summary>
    public class ApiEntryPointDescriptionBuilder : IApiEntryPointDescriptionBuilder
    {
        private readonly IHttpServerConfiguration _httpServerConfiguration;
        private readonly IApiDescriptionBuilderFactory _apiDescriptionBuilderFactory;
        private readonly IEnumerable<IHttpControllerDescriptionBuilder> _controllerDescriptionBuilders;

        /// <summary>Initializes a new instance of the <see cref="ApiEntryPointDescriptionBuilder"/> class.</summary>
        /// <param name="httpServerConfiguration">HTTP server configuration with base Url.</param>
        /// <param name="apiDescriptionBuilderFactory">The API description builder factory.</param>
        /// <param name="controllerDescriptionBuilders">The controller description builders.</param>
        [ExcludeFromCodeCoverage]
        public ApiEntryPointDescriptionBuilder(
            IHttpServerConfiguration httpServerConfiguration,
            IApiDescriptionBuilderFactory apiDescriptionBuilderFactory,
            IEnumerable<IHttpControllerDescriptionBuilder> controllerDescriptionBuilders)
        {
            if (httpServerConfiguration == null)
            {
                throw new ArgumentNullException("httpServerConfiguration");
            }

            if (apiDescriptionBuilderFactory == null)
            {
                throw new ArgumentNullException("apiDescriptionBuilderFactory");
            }

            if (controllerDescriptionBuilders == null)
            {
                throw new ArgumentNullException("controllerDescriptionBuilders");
            }

            if (!controllerDescriptionBuilders.Any())
            {
                throw new ArgumentOutOfRangeException("controllerDescriptionBuilders");
            }

            _httpServerConfiguration = httpServerConfiguration;
            _apiDescriptionBuilderFactory = apiDescriptionBuilderFactory;
            _controllerDescriptionBuilders = controllerDescriptionBuilders;
        }

        /// <inheritdoc />
        public Type SpecializationType { get { return typeof(IApiDocumentation); } }

        /// <inheritdoc />
        public Url EntryPoint { get; set; }

        /// <inheritdoc />
        public void BuildDescription(IApiDocumentation apiDocumentation, IEnumerable<Uri> profiles)
        {
            ControllerInfo entryPointControllerInfo = null;
            foreach (var controllerDescriptionBuilder in _controllerDescriptionBuilders)
            {
                Type targetImplementation;
                var controllerInfo = GetTargetControllerInfo(controllerDescriptionBuilder, out targetImplementation, out entryPointControllerInfo);
                if (controllerInfo == null)
                {
                    continue;
                }

                var apiDescriptionBuilder = _apiDescriptionBuilderFactory.Create(targetImplementation);
                apiDescriptionBuilder.BuildDescription(apiDocumentation, profiles);
            }

            if (entryPointControllerInfo != null)
            {
                BuildEntryPointDescription(apiDocumentation, entryPointControllerInfo);
            }
        }

        /// <inheritdoc />
        public void BuildOperationDescription(IEntity entryPointEntity, OperationInfo<Verb> operationInfo, IEnumerable<Uri> profiles)
        {
            foreach (var controllerDescriptionBuilder in _controllerDescriptionBuilders)
            {
                Type targetImplementation;
                var controllerInfo = GetTargetControllerInfo(controllerDescriptionBuilder, out targetImplementation);
                if ((controllerInfo == null) || (!controllerInfo.Operations.Any(operation => operation.Equals(operationInfo))))
                {
                    continue;
                }

                var apiDescriptionBuilder = _apiDescriptionBuilderFactory.Create(targetImplementation);
                apiDescriptionBuilder.BuildOperationDescription(entryPointEntity, operationInfo, profiles);
            }
        }

        private ControllerInfo GetTargetControllerInfo(IHttpControllerDescriptionBuilder controllerDescriptionBuilder, out Type targetImplementation)
        {
            ControllerInfo entryPointControllerInfo;
            return GetTargetControllerInfo(controllerDescriptionBuilder, out targetImplementation, out entryPointControllerInfo);
        }

        private ControllerInfo GetTargetControllerInfo(IHttpControllerDescriptionBuilder controllerDescriptionBuilder, out Type targetImplementation, out ControllerInfo entryPointControllerInfo)
        {
            entryPointControllerInfo = null;
            targetImplementation = null;
            Type possibleImplementation = null;
            var controllerDescriptionBuilderType = controllerDescriptionBuilder.GetType();
            controllerDescriptionBuilderType.GetInterfaceImplementation(
                typeof(IHttpControllerDescriptionBuilder<>),
                candidateInterface => typeof(IController).GetTypeInfo().IsAssignableFrom(possibleImplementation = candidateInterface.GetGenericArguments()[0]));
            if ((possibleImplementation == null) || ((possibleImplementation.GetTypeInfo().IsGenericType) && (possibleImplementation.GetGenericTypeDefinition() == typeof(DescriptionController<>))))
            {
                return null;
            }

            targetImplementation = possibleImplementation;
            var controllerDescription = controllerDescriptionBuilder.BuildDescriptor();
            if (controllerDescriptionBuilder is IHttpControllerDescriptionBuilder<EntryPointDescriptionController>)
            {
                entryPointControllerInfo = controllerDescription;
                return null;
            }

            if ((controllerDescription.EntryPoint == null) || (controllerDescription.EntryPoint.Url.ToString() != EntryPoint.ToString()))
            {
                return null;
            }

            return controllerDescription;
        }

        private void BuildEntryPointDescription(IApiDocumentation apiDocumentation, ControllerInfo entryPointControllerInfo)
        {
            var classUri = apiDocumentation.Context.Mappings.FindEntityMappingFor<IApiDocumentation>(null).Classes.Select(item => item.Term).FirstOrDefault();
            var apiDocumentationClass = apiDocumentation.Context.Create<IClass>(classUri);
            foreach (OperationInfo<Verb> operation in entryPointControllerInfo.Operations)
            {
                var url = (Uri)operation.Url;
                if (operation.UrlTemplate != null)
                {
                    var template = new UriTemplate(_httpServerConfiguration.BaseUri + operation.UrlTemplate.TrimStart('/'));
                    var variables = operation.UnderlyingMethod.GetParameters().ToDictionary(parameter => parameter.Name, parameter => (object)parameter.DefaultValue.ToString());
                    template.AddParameters(variables);
                    url = new Uri(template.Resolve());
                }

                var operationId = new Iri((Uri)((HttpUrl)_httpServerConfiguration.BaseUri + url.ToString()));
                var supportedOperation = operation.AsOperation(_httpServerConfiguration.BaseUri, apiDocumentation, operationId);
                supportedOperation.MediaTypes.AddRange(ApiDescriptionBuilder.RdfMediaTypes);
                supportedOperation.Label = operation.UnderlyingMethod.Name;
                supportedOperation.Method.Add(operation.ProtocolSpecificCommand.ToString());
                var returned = apiDocumentation.Context.Create<IClass>(new Iri());
                returned.SubClassOf.Add(apiDocumentationClass);
                supportedOperation.Returns.Add(returned);
                apiDocumentationClass.SupportedOperations.Add(supportedOperation);
            }

            apiDocumentation.SupportedClasses.Add(apiDocumentationClass);
        }
    }
}