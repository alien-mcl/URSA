using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RomanticWeb.Entities;
using URSA.Configuration;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Description.Entities;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides helper methods extending hypermedia driven controllers with hypermedia injection facility.</summary>
    public static class HypermediaDrivenControllerExtensions
    {
        static HypermediaDrivenControllerExtensions()
        {
            var container = UrsaConfigurationSection.InitializeComponentProvider();
            EntityContextProvider = () => container.Resolve<IEntityContextProvider>();
            ControllerDescriptionBuilder = type => (IHttpControllerDescriptionBuilder)container.Resolve(typeof(IHttpControllerDescriptionBuilder<>).MakeGenericType(type));
            ApiDescriptionBuilder = type => (IApiDescriptionBuilder)container.Resolve(typeof(IApiDescriptionBuilder<>).MakeGenericType(type));
        }

        internal static Func<IEntityContextProvider> EntityContextProvider { get; set; }

        internal static Func<Type, IHttpControllerDescriptionBuilder> ControllerDescriptionBuilder { get; set; }

        internal static Func<Type, IApiDescriptionBuilder> ApiDescriptionBuilder { get; set; }

        /// <summary>Instructs the framework to inject that a given <paramref name="operation" /> should be injected into the payload</summary>
        /// <typeparam name="TController">Type of the controller.</typeparam>
        /// <typeparam name="TEntity">Type of the entity being controlled.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="operation">The operation description to be injected.</param>
        /// <returns>Same instance of the controller.</returns>
        public static TController Inject<TController, TEntity>(this TController controller, Expression<Action<TController>> operation)
            where TController : class, IHypermediaDrivenController<TController, TEntity>
            where TEntity : IEntity
        {
            if (controller == null)
            {
                return null;
            }

            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            controller.InjectOperation(operation.Body.UnpackMethodSignature());
            return controller;
        }

        /// <summary>Instructs the framework to inject that a given <paramref name="operation" /> should be injected into the payload</summary>
        /// <typeparam name="TController">Type of the controller.</typeparam>
        /// <typeparam name="TEntity">Type of the entity being controlled.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="operation">The operation description to be injected.</param>
        /// <returns>Same instance of the controller.</returns>
        public static TController InjectAsync<TController, TEntity>(this TController controller, Expression<Action<TController>> operation)
            where TController : class, IHypermediaDrivenAsyncController<TController, TEntity>
            where TEntity : IEntity
        {
            if (controller == null)
            {
                return null;
            }

            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            controller.InjectOperation(operation.Body.UnpackMethodSignature());
            return controller;
        }

        internal static void InjectOperation(this Type controllerType, MethodInfo methodInfo, IRequestInfo request)
        {
            var entityContextProvider = EntityContextProvider();
            var controllerDescriptionBuilder = ControllerDescriptionBuilder(controllerType);
            var apiDescriptionBuilder = ApiDescriptionBuilder(controllerType);
            var hook = entityContextProvider.EntityContext.Create<IEntity>(new EntityId((Uri)request.Url));
            var targetOperation = (OperationInfo<Verb>)controllerDescriptionBuilder.BuildDescriptor().Operations.First(operation => operation.UnderlyingMethod == methodInfo);
            apiDescriptionBuilder.BuildOperationDescription(hook, targetOperation, request.GetRequestedMediaTypeProfiles());
            entityContextProvider.EntityContext.Commit();
        }

        private static void InjectOperation<TController>(this TController controller, MethodInfo methodInfo)
            where TController : IController
        {
            typeof(TController).InjectOperation(methodInfo, controller.Response.Request);
        }

        private static MethodInfo UnpackMethodSignature(this Expression operation)
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