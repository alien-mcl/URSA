using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RDeF.Entities;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Configuration;
using URSA.Web.Http.Description.Entities;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a basic implementation of the <see cref="IHypermediaFacility" />.</summary>
    public class HypermediaFacility : IHypermediaFacility
    {
        private readonly IController _controller;
        private readonly IEntityContext _entityContext;
        private readonly IHttpControllerDescriptionBuilder _controllerDescriptionBuilder;
        private readonly IApiDescriptionBuilder _apiDescriptionBuilder;
        private readonly IHttpServerConfiguration _httpServerConfiguration;

        /// <summary>Initializes a new instance of the <see cref="HypermediaFacility" /> class.</summary>
        /// <param name="controller">Owning controller.</param>
        /// <param name="entityContext">Entity context.</param>
        /// <param name="controllerDescriptionBuilder">Controller description builder.</param>
        /// <param name="apiDescriptionBuilder">API description builder.</param>
        /// <param name="httpServerConfiguration">HTTP server configuration with base Uri.</param>
        public HypermediaFacility(
            IController controller,
            IEntityContext entityContext,
            IHttpControllerDescriptionBuilder controllerDescriptionBuilder,
            IApiDescriptionBuilder apiDescriptionBuilder,
            IHttpServerConfiguration httpServerConfiguration)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller");
            }

            if (entityContext == null)
            {
                throw new ArgumentNullException("entityContext");
            }

            if (controllerDescriptionBuilder == null)
            {
                throw new ArgumentNullException("controllerDescriptionBuilder");
            }

            if (apiDescriptionBuilder == null)
            {
                throw new ArgumentNullException("apiDescriptionBuilder");
            }

            if (httpServerConfiguration == null)
            {
                throw new ArgumentNullException("httpServerConfiguration");
            }

            _controller = controller;
            _entityContext = entityContext;
            _controllerDescriptionBuilder = controllerDescriptionBuilder;
            _apiDescriptionBuilder = apiDescriptionBuilder;
            _httpServerConfiguration = httpServerConfiguration;
        }

        /// <summary>Instructs the framework to inject that a given <paramref name="operation" /> should be injected into the payload</summary>
        /// <typeparam name="TController">Type of the controller.</typeparam>
        /// <param name="operation">The operation description to be injected.</param>
        public void Inject<TController>(Expression<Action<TController>> operation)
            where TController : IController
        {
            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            InjectOperation(UnpackMethodSignature(operation.Body));
        }

        private void InjectOperation(MethodInfo methodInfo)
        {
            var hypermediaControls = new OperationHypermediaControl(
                HypermediaControlRules.Include,
                (OperationInfo<Verb>)_controllerDescriptionBuilder.BuildDescriptor().Operations.First(operation => operation.UnderlyingMethod == methodInfo),
                _apiDescriptionBuilder,
                _entityContext,
                _httpServerConfiguration);
            _controller.Response.Request.HypermediaControls.Add(hypermediaControls);
        }

        private MethodInfo UnpackMethodSignature(Expression operation)
        {
            var methodCall = operation as MethodCallExpression;
            if (methodCall == null)
            {
                throw new ArgumentOutOfRangeException("operation");
            }

            return methodCall.Method;
        }
    }
}