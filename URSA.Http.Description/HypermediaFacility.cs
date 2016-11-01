using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Description.Entities;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a basic implementation of the <see cref="IHypermediaFacility" />.</summary>
    public class HypermediaFacility : IHypermediaFacility
    {
        private readonly IController _controller;
        private readonly IEntityContextProvider _entityContextProvider;
        private readonly IHttpControllerDescriptionBuilder _controllerDescriptionBuilder;
        private readonly IApiDescriptionBuilder _apiDescriptionBuilder;

        /// <summary>Initializes a new instance of the <see cref="HypermediaFacility" /> class.</summary>
        /// <param name="controller">Owning controller.</param>
        /// <param name="entityContextProvider">Entity context provider.</param>
        /// <param name="controllerDescriptionBuilder">Controller description builder.</param>
        /// <param name="apiDescriptionBuilder">API description builder.</param>
        public HypermediaFacility(
            IController controller,
            IEntityContextProvider entityContextProvider,
            IHttpControllerDescriptionBuilder controllerDescriptionBuilder,
            IApiDescriptionBuilder apiDescriptionBuilder)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller");
            }

            if (entityContextProvider == null)
            {
                throw new ArgumentNullException("entityContextProvider");
            }

            if (controllerDescriptionBuilder == null)
            {
                throw new ArgumentNullException("controllerDescriptionBuilder");
            }

            if (apiDescriptionBuilder == null)
            {
                throw new ArgumentNullException("apiDescriptionBuilder");
            }

            _controller = controller;
            _entityContextProvider = entityContextProvider;
            _controllerDescriptionBuilder = controllerDescriptionBuilder;
            _apiDescriptionBuilder = apiDescriptionBuilder;
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
                _entityContextProvider);
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