using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using RomanticWeb;
using RomanticWeb.DotNetRDF;
using System;
using System.Configuration;
using System.Linq;
using System.Web;
using URSA.CastleWindsor.ComponentModel;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Description;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;
using VDS.RDF;

namespace URSA.CastleWindsor
{
    /// <summary>Installs HTTP components.</summary>
    public class HttpInstaller : IWindsorInstaller
    {
        private Lazy<EntityContextFactory> _entityContextFactory;
        private object _lock = new object();

        /// <summary>Initializes a new instance of the <see cref="HttpInstaller" /> class.</summary>
        public HttpInstaller()
        {
            _entityContextFactory = new Lazy<EntityContextFactory>(CreateEntityContextFactory);
        }

        /// <inheritdoc />
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            WindsorComponentProvider componentProvider = container.Resolve<WindsorComponentProvider>();
            var configuration = (HttpConfigurationSection)ConfigurationManager.GetSection(HttpConfigurationSection.ConfigurationSection);
            Type sourceSelectorType = ((configuration != null) && (configuration.DefaultParameterSourceSelectorType != null) ?
                configuration.DefaultParameterSourceSelectorType :
                typeof(DefaultParameterSourceSelector));

            container.Register(Component.For<IEntityContext>().UsingFactoryMethod(CreateEntityContext).LifestylePerWebRequest());
            container.Register(Component.For<IEntityContextFactory>().Instance(_entityContextFactory.Value).LifestyleSingleton());
            container.Register(Component.For<IDefaultParameterSourceSelector>().ImplementedBy(sourceSelectorType).LifestyleSingleton());
            container.Register(Component.For(typeof(IControllerDescriptionBuilder<>)).Forward(typeof(IHttpControllerDescriptionBuilder<>))
                .Forward<IControllerDescriptionBuilder>().Forward<IHttpControllerDescriptionBuilder>()
                .ImplementedBy(typeof(ControllerDescriptionBuilder<>), componentProvider.GenericImplementationMatchingStrategy).LifestyleTransient());
            container.Register(Component.For(typeof(DescriptionController<>)).Forward<IController>()
                .ImplementedBy(typeof(DescriptionController<>), componentProvider.GenericImplementationMatchingStrategy).LifestyleTransient());
            container.Register(Component.For<IParameterSourceArgumentBinder>().ImplementedBy<FromQueryStringArgumentBinder>().Activator<NonPublicComponentActivator>().LifestyleSingleton());
            container.Register(Component.For<IParameterSourceArgumentBinder>().ImplementedBy<FromUriArgumentBinder>().Activator<NonPublicComponentActivator>().LifestyleSingleton());
            container.Register(Component.For<IParameterSourceArgumentBinder>().ImplementedBy<FromBodyArgumentBinder>().Activator<NonPublicComponentActivator>().LifestyleSingleton());
        }

        private IEntityContext CreateEntityContext(IKernel kernel, CreationContext context)
        {
            IEntityContext result = null;
            lock (_lock)
            {
                result = _entityContextFactory.Value.WithDotNetRDF(new TripleStore())
                    .WithBaseUri(policy => policy.Default.Is(new Uri(String.Format("{0}://{1}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host))))
                    .CreateContext();
            }

            return result;
        }

        private EntityContextFactory CreateEntityContextFactory()
        {
            return EntityContextFactory.FromConfiguration("http").WithDefaultOntologies();
        }
    }
}