using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using RomanticWeb;
using RomanticWeb.DotNetRDF;
using System;
using System.Configuration;
using System.Reflection;
using System.Web;
using Castle.Facilities.TypedFactory;
using RomanticWeb.Configuration;
using RomanticWeb.NamedGraphs;
using URSA.CastleWindsor.ComponentModel;
using URSA.CodeGen;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.CodeGen;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.Mapping;
using URSA.Web.Http.Description.NamedGraphs;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;
using VDS.RDF;

namespace URSA.CastleWindsor
{
    /// <summary>Installs HTTP components.</summary>
    public class HttpInstaller : IWindsorInstaller
    {
        private readonly Lazy<ITripleStore> _tripleStore; 
        private readonly Lazy<EntityContextFactory> _entityContextFactory;
        private readonly object _lock = new object();

        /// <summary>Initializes a new instance of the <see cref="HttpInstaller" /> class.</summary>
        public HttpInstaller()
        {
            _tripleStore = new Lazy<ITripleStore>(CreateTripleStore, true);
            _entityContextFactory = new Lazy<EntityContextFactory>(CreateEntityContextFactory, true);
        }

        /// <inheritdoc />
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            WindsorComponentProvider componentProvider = container.Resolve<WindsorComponentProvider>();
            var configuration = (HttpConfigurationSection)ConfigurationManager.GetSection(HttpConfigurationSection.ConfigurationSection);
            Type sourceSelectorType = ((configuration != null) && (configuration.DefaultValueRelationSelectorType != null) ?
                configuration.DefaultValueRelationSelectorType :
                typeof(DefaultValueRelationSelector));
            container.AddFacility<TypedFactoryFacility>();
            var typedFactory = new UrsaCustomTypedFactory();
            container.Register(Component.For<IControllerActivator>().UsingFactoryMethod((kernel, context) => new DefaultControllerActivator(UrsaConfigurationSection.InitializeComponentProvider())).LifestyleSingleton());
            container.Register(Component.For<ITripleStore>().Instance(_tripleStore.Value).Named("InMemoryTripleStore").LifestyleSingleton());
            container.Register(Component.For<INamedGraphSelectorFactory>().AsFactory(typedFactory).LifestyleSingleton());
            container.Register(Component.For<INamedGraphSelector>().ImplementedBy<LocallyControlledOwningResourceNamedGraphSelector>()
                .Forward<ILocallyControlledNamedGraphSelector>().Named("InMemoryNamedGraphSelector").LifestyleSingleton());
            container.Register(Component.For<IEntityContextFactory>().Instance(_entityContextFactory.Value).Named("InMemoryEntityContextFactory").LifestyleSingleton());
            container.Register(Component.For<IEntityContext>().UsingFactoryMethod(CreateEntityContext).Named("InMemoryEntityContext").LifeStyle.HybridPerWebRequestPerThread());
            container.Register(Component.For<IEntityContextProvider>().AsFactory(typedFactory).LifestyleSingleton());
            container.Register(Component.For<IDefaultValueRelationSelector>().ImplementedBy(sourceSelectorType).LifestyleSingleton());
            container.Register(Component.For(typeof(DescriptionController<>)).Forward<IController>()
                .ImplementedBy(typeof(DescriptionController<>), componentProvider.GenericImplementationMatchingStrategy).LifestyleTransient());
            container.Register(Component.For<EntryPointDescriptionController>().Forward<IController>().ImplementedBy<EntryPointDescriptionController>().LifestyleTransient());
            container.Register(Component.For<IParameterSourceArgumentBinder>().ImplementedBy<FromQueryStringArgumentBinder>().Activator<NonPublicComponentActivator>().LifestyleSingleton());
            container.Register(Component.For<IParameterSourceArgumentBinder>().ImplementedBy<FromUriArgumentBinder>().Activator<NonPublicComponentActivator>().LifestyleSingleton());
            container.Register(Component.For<IParameterSourceArgumentBinder>().ImplementedBy<FromBodyArgumentBinder>().Activator<NonPublicComponentActivator>().LifestyleSingleton());
            container.Register(Component.For<IClassGenerator>().ImplementedBy<HydraClassGenerator>().LifestyleSingleton());
            container.Register(Component.For<IUriParser>().ImplementedBy<URSA.Web.Http.Description.CodeGen.GenericUriParser>().LifestyleSingleton());
            container.Register(Component.For<IUriParser>().ImplementedBy<HydraUriParser>().LifestyleSingleton().Named(typeof(HydraUriParser).FullName));
            container.Register(Component.For<IUriParser>().ImplementedBy<XsdUriParser>().LifestyleSingleton().Named(typeof(XsdUriParser).FullName));
            container.Register(Component.For<IUriParser>().ImplementedBy<OGuidUriParser>().LifestyleSingleton().Named(typeof(OGuidUriParser).FullName));
            container.Register(Component.For<IXmlDocProvider>().ImplementedBy<XmlDocProvider>().LifestyleSingleton());
            container.Register(Component.For<IWebRequestProvider>().ImplementedBy<WebRequestProvider>().LifestyleSingleton());
            container.Register(Component.For<IRequestHandler<RequestInfo, ResponseInfo>>().ImplementedBy<RequestHandler>().LifestyleSingleton());
            container.Register(Component.For<IResponseComposer>().ImplementedBy<ResponseComposer>().LifestyleSingleton());
            container.Register(Component.For<IDelegateMapper<RequestInfo>>().ImplementedBy<DelegateMapper>().LifestyleSingleton());
            container.Register(Component.For<IArgumentBinder<RequestInfo>>().ImplementedBy<ArgumentBinder>().LifestyleSingleton());
            container.Register(Component.For<IResultBinder<RequestInfo>>().ImplementedBy<ResultBinder>().LifestyleSingleton());
            container.Register(Component.For<ITypeDescriptionBuilder>().ImplementedBy<HydraCompliantTypeDescriptionBuilder>()
                .Named(EntityConverter.Hydra.ToString()).IsDefault().LifestyleSingleton());
            container.Register(Component.For<IServerBehaviorAttributeVisitor>().ImplementedBy<DescriptionBuildingServerBahaviorAttributeVisitor<ParameterInfo>>().Named("Hydra"));
            container.Register(Component.For(typeof(IApiDescriptionBuilder<>)).ImplementedBy(typeof(ApiDescriptionBuilder<>)).LifestyleSingleton().Forward<IApiDescriptionBuilder>());
            container.Register(Component.For<IApiEntryPointDescriptionBuilder>().ImplementedBy<ApiEntryPointDescriptionBuilder>().LifestyleSingleton().Forward<IApiDescriptionBuilder>());
            container.Register(Component.For<IApiDescriptionBuilderFactory>().AsFactory(typedFactory).LifestyleSingleton());
        }

        private static Uri GetBaseUri()
        {
            try
            {
                if (HttpContext.Current == null)
                {
                    return null;
                }

                var baseUrl = String.Format(
                    "{0}://{1}{2}/",
                    HttpContext.Current.Request.Url.Scheme,
                    HttpContext.Current.Request.Url.Host,
                    ((HttpContext.Current.Request.Url.Port != 80) && (HttpContext.Current.Request.Url.Port > 0) ? String.Format(":{0}", HttpContext.Current.Request.Url.Port) : String.Empty));
                return new Uri(baseUrl);
            }
            catch (HttpException)
            {
                return null;
            }
        }

        private IEntityContext CreateEntityContext(IKernel kernel, CreationContext context)
        {
            IEntityContext result = null;
            lock (_lock)
            {
                EntityContextFactory entityContextFactory = _entityContextFactory.Value;
                if (!kernel.HasComponent("BaseUri"))
                {
                    var baseUri = GetBaseUri();
                    if (baseUri != null)
                    {
                        kernel.Register(Component.For<Uri>().Named("BaseUri").Instance(baseUri));
                        result = entityContextFactory
                            .WithNamedGraphSelector(kernel.Resolve<INamedGraphSelector>())
                            .WithBaseUri(policy => policy.Default.Is(baseUri))
                            .CreateContext();
                    }
                }

                if (result == null)
                {
                    result = entityContextFactory.CreateContext();
                }
            }

            return result;
        }

        private ITripleStore CreateTripleStore()
        {
            var metaGraphUri = ConfigurationSectionHandler.Default.Factories[DescriptionConfigurationSection.Default.DefaultStoreFactoryName].MetaGraphUri;
            var store = new ThreadSafeTripleStore();
            var metaGraph = new ThreadSafeGraph() { BaseUri = metaGraphUri };
            store.Add(metaGraph);
            return store;
        }

        private EntityContextFactory CreateEntityContextFactory()
        {
            return EntityContextFactory
                .FromConfiguration(DescriptionConfigurationSection.Default.DefaultStoreFactoryName)
                .WithDefaultOntologies()
                .WithDotNetRDF(_tripleStore.Value);
        }
    }
}