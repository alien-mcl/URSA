using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using URSA.Web.Description;
using URSA.Web.Description.Http;

namespace URSA.Web.Http
{
    /// <summary>Provides a default implementation of the <see cref="IDelegateMapper{RequestInfo}"/> interface.</summary>
    public class DelegateMapper : IDelegateMapper<RequestInfo>
    {
        private IEnumerable<IHttpControllerDescriptionBuilder> _httpControllerDescriptionBuilders;
        private Lazy<IEnumerable<ControllerInfo>> _controllerDescriptors;
        private IControllerActivator _controllerActivator;

        /// <summary>Initializes a new instance of the <see cref="DelegateMapper" /> class.</summary>
        /// <param name="httpControllerDescriptionBuilders">HTTP controller description builders.</param>
        /// <param name="controllerInstanceFactoryMethod">Method creating instances of <see cref="IController" />s.</param>
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
            return (from controller in _controllerDescriptors.Value
                    from operation in controller.Operations.Cast<OperationInfo<Verb>>()
                    where (request.Method == operation.ProtocolSpecificCommand) && (operation.TemplateRegex.IsMatch(requestUri))
                    let controllerType = controller.GetType().GetGenericArguments()[0]
                    select new RequestMapping(_controllerActivator.CreateInstance(controllerType), operation, operation.Uri))
                   .FirstOrDefault();
        }

        private IEnumerable<ControllerInfo> BuildControllerDescriptors()
        {
            return _httpControllerDescriptionBuilders.Select(item => item.BuildDescriptor());
        }
    }
}