using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Description;

namespace URSA.Web.Http
{
    /// <summary>Provides a default implementation of the <see cref="IDelegateMapper{RequestInfo}"/> interface.</summary>
    public class DelegateMapper : IDelegateMapper<RequestInfo>
    {
        private readonly IEnumerable<IHttpControllerDescriptionBuilder> _httpControllerDescriptionBuilders;
        private readonly Lazy<IEnumerable<ControllerInfo>> _controllerDescriptors;
        private readonly IControllerActivator _controllerActivator;

        /// <summary>Initializes a new instance of the <see cref="DelegateMapper" /> class.</summary>
        /// <param name="httpControllerDescriptionBuilders">HTTP controller description builders.</param>
        /// <param name="controllerActivator">Method creating instances of <see cref="IController" />s.</param>
        [ExcludeFromCodeCoverage]
        public DelegateMapper(IEnumerable<IHttpControllerDescriptionBuilder> httpControllerDescriptionBuilders, IControllerActivator controllerActivator)
        {
            if (httpControllerDescriptionBuilders == null)
            {
                throw new ArgumentNullException("httpControllerDescriptionBuilders");
            }

            if (controllerActivator == null)
            {
                throw new ArgumentNullException("controllerActivator");
            }

            _httpControllerDescriptionBuilders = httpControllerDescriptionBuilders;
            _controllerActivator = controllerActivator;
            _controllerDescriptors = new Lazy<IEnumerable<ControllerInfo>>(BuildControllerDescriptors);
        }

        /// <inheritdoc />
        IRequestMapping IDelegateMapper.MapRequest(IRequestInfo request)
        {
            if (!(request is RequestInfo))
            {
                throw new ArgumentOutOfRangeException("request");
            }

            return MapRequest((RequestInfo)request);
        }

        /// <inheritdoc />
        public IRequestMapping MapRequest(RequestInfo request)
        {
            string requestUri = request.Uri.ToRelativeUri().ToString();
            var allowedOptions = new List<OperationInfo<Verb>>();
            foreach (var controller in _controllerDescriptors.Value)
            {
                foreach (var operation in controller.Operations.Cast<OperationInfo<Verb>>().Where(operation => operation.TemplateRegex.IsMatch(requestUri)))
                {
                    allowedOptions.Add(operation);
                    if (request.Method != operation.ProtocolSpecificCommand)
                    {
                        continue;
                    }

                    return new RequestMapping(_controllerActivator.CreateInstance(controller.GetType().GetGenericArguments()[0]), operation, operation.Uri);
                }
            }

            if (allowedOptions.Count <= 0)
            {
                return null;
            }

            var option = allowedOptions.First();
            return new OptionsRequestMapping(
                OptionsController.CreateOperationInfo(option),
                option.Uri,
                allowedOptions.Select(item => item.ProtocolSpecificCommand.ToString()).ToArray());
        }

        private IEnumerable<ControllerInfo> BuildControllerDescriptors()
        {
            return _httpControllerDescriptionBuilders.Select(item => item.BuildDescriptor());
        }
    }
}