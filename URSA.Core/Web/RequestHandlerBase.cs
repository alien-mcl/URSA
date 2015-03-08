using System;
using System.Configuration;
using System.Reflection;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web.Converters;
using URSA.Web.Mapping;

namespace URSA.Web
{
    /// <summary>Serves as an abstract of the main entry point for the URSA.</summary>
    /// <typeparam name="T">Type of the requests.</typeparam>
    /// <typeparam name="R">Type of the response.</typeparam>
    public abstract class RequestHandlerBase<T, R> : IRequestHandler<T, R>
        where T : IRequestInfo
        where R : IResponseInfo
    {
        /// <summary>Initializes a new instance of the <see cref="RequestHandlerBase{T,R}"/> class.</summary>
        public RequestHandlerBase()
        {
            UrsaConfigurationSection configuration = (UrsaConfigurationSection)ConfigurationManager.GetSection(UrsaConfigurationSection.ConfigurationSection);
            if (configuration == null)
            {
                throw new InvalidOperationException(String.Format("Cannot instantiate a '{0}' without a proper '{1}' configuration.", GetType(), typeof(ComponentModel.IComponentProvider)));
            }

            BaseInitialize(
                UrsaConfigurationSection.InitializeComponentProvider(),
                configuration.GetProvider<IControllerActivator>(configuration.ControllerActivatorType ?? typeof(DefaultControllerActivator), typeof(IComponentProvider)));
        }

        /// <summary>Gets or sets the argument binding facility.</summary>
        protected IArgumentBinder ArgumentBinder { get; set; }

        /// <summary>Gets or sets the handler mapper.</summary>
        protected IDelegateMapper HandlerMapper { get; set; }

        /// <summary>Gets the converters provider.</summary>
        protected IConverterProvider ConverterProvider { get; private set; }

        /// <summary>Gets the dependency injection container.</summary>
        protected IComponentProvider Container { get; private set; }

        /// <inheritdoc />
        public R HandleRequest(T request)
        {
            var action = HandlerMapper.MapRequest(request);
            return HandleRequest(request, action);
        }

        /// <summary>Exposes the request handling routine to the sub classes.</summary>
        /// <param name="request">Request details.</param>
        /// <param name="requestMapping">Request mapping.</param>
        /// <returns>Response descriptor.</returns>
        protected abstract R HandleRequest(T request, IRequestMapping requestMapping);

        /// <summary>Initializes the instance of the request handler.</summary>
        protected abstract void Initialize();
         
        private void BaseInitialize(IComponentProvider container, ConstructorInfo controllerActivatorCtor)
        {
            Container = container;
            ConverterProvider = Container.Resolve<IConverterProvider>();
            var controllerActivator = (IControllerActivator)controllerActivatorCtor.Invoke(new object[] { Container });
            Container.Register(controllerActivator);
            Initialize();
            if (HandlerMapper == null)
            {
                HandlerMapper = Container.Resolve<IDelegateMapper<T>>();
            }

            if (ArgumentBinder == null)
            {
                ArgumentBinder = Container.Resolve<IArgumentBinder<T>>();
            }
        }
    }
}