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
    /// <typeparam name="TR">Type of the response.</typeparam>
    public abstract class RequestHandlerBase<T, TR> : IRequestHandler<T, TR>
        where T : IRequestInfo
        where TR : IResponseInfo
    {
        /// <summary>Initializes a new instance of the <see cref="RequestHandlerBase{T,TR}"/> class.</summary>
        protected RequestHandlerBase()
        {
            UrsaConfigurationSection configuration = (UrsaConfigurationSection)ConfigurationManager.GetSection(UrsaConfigurationSection.ConfigurationSection);
            if (configuration == null)
            {
                throw new InvalidOperationException(String.Format("Cannot instantiate a '{0}' without a proper '{1}' configuration.", GetType(), typeof(ComponentModel.IComponentProvider)));
            }

            var container = UrsaConfigurationSection.InitializeComponentProvider();
            ConverterProvider = container.Resolve<IConverterProvider>();
            container.Register((IControllerActivator)configuration.GetProvider<IControllerActivator>(configuration.ControllerActivatorType ?? typeof(DefaultControllerActivator), typeof(IComponentProvider))
                .Invoke(new object[] { container }));
        }

        /// <summary>Gets the converters provider.</summary>
        protected IConverterProvider ConverterProvider { get; private set; }

        /// <inheritdoc />
        public abstract TR HandleRequest(T request);
    }
}