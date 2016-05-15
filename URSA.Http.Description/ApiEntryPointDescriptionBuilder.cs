using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Resta.UriTemplates;
using RomanticWeb.Entities;
using URSA.Reflection;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides an API entry point description.</summary>
    public class ApiEntryPointDescriptionBuilder : IApiEntryPointDescriptionBuilder
    {
        private readonly IApiDescriptionBuilderFactory _apiDescriptionBuilderFactory;
        private readonly IEnumerable<IHttpControllerDescriptionBuilder> _controllerDescriptionBuilders;

        /// <summary>Initializes a new instance of the <see cref="ApiEntryPointDescriptionBuilder"/> class.</summary>
        /// <param name="apiDescriptionBuilderFactory">The API description builder factory.</param>
        /// <param name="controllerDescriptionBuilders">The controller description builders.</param>
        [ExcludeFromCodeCoverage]
        public ApiEntryPointDescriptionBuilder(IApiDescriptionBuilderFactory apiDescriptionBuilderFactory, IEnumerable<IHttpControllerDescriptionBuilder> controllerDescriptionBuilders)
        {
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
                candidateInterface => typeof(IController).IsAssignableFrom(possibleImplementation = candidateInterface.GetGenericArguments()[0]));
            if ((possibleImplementation == null) || ((possibleImplementation.IsGenericType) && (possibleImplementation.GetGenericTypeDefinition() == typeof(DescriptionController<>))))
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
            var classUri = apiDocumentation.Context.Mappings.MappingFor(typeof(IApiDocumentation)).Classes.Select(item => item.Uri).FirstOrDefault();
            var apiDocumentationClass = apiDocumentation.Context.Create<IClass>(classUri);
            var baseUri = apiDocumentation.Context.BaseUriSelector.SelectBaseUri(new EntityId(new Uri("/", UriKind.Relative)));
            foreach (OperationInfo<Verb> operation in entryPointControllerInfo.Operations)
            {
                var url = (Uri)operation.Url;
                if (operation.UrlTemplate != null)
                {
                    var template = new UriTemplate(baseUri + operation.UrlTemplate.TrimStart('/'));
                    var variables = operation.UnderlyingMethod.GetParameters().ToDictionary(parameter => parameter.Name, parameter => (object)parameter.DefaultValue.ToString());
                    url = template.ResolveUri(variables);
                }

                var operationId = new EntityId(url.Combine(baseUri));
                var supportedOperation = operation.AsOperation(apiDocumentation, operationId);
                supportedOperation.MediaTypes.AddRange(ApiDescriptionBuilder.RdfMediaTypes);
                supportedOperation.Label = operation.UnderlyingMethod.Name;
                supportedOperation.Method.Add(operation.ProtocolSpecificCommand.ToString());
                var returned = apiDocumentation.Context.Create<IClass>(apiDocumentation.CreateBlankId());
                returned.SubClassOf.Add(apiDocumentationClass);
                supportedOperation.Returns.Add(returned);
                apiDocumentationClass.SupportedOperations.Add(supportedOperation);
            }

            apiDocumentation.SupportedClasses.Add(apiDocumentationClass);
        }
    }
}