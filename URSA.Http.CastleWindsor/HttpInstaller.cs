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
using System.Reflection;
using Castle.Facilities.TypedFactory;
using RomanticWeb.Configuration;
using RomanticWeb.NamedGraphs;
using URSA.CastleWindsor.ComponentModel;
using URSA.CodeGen;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web;
using URSA.Web.Converters;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Configuration;
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
        private static readonly Uri MetaGraphUri = ConfigurationSectionHandler.Default.Factories[DescriptionConfigurationSection.Default.DefaultStoreFactoryName].MetaGraphUri;
        private readonly Lazy<EntityContextFactory> _entityContextFactory;
        private readonly INamedGraphSelector _namedGraphSelector;
        private readonly object _lock = new object();
        private bool _isBaseUriInitialized;

        /// <summary>Initializes a new instance of the <see cref="HttpInstaller" /> class.</summary>
        public HttpInstaller()
        {
            _entityContextFactory = new Lazy<EntityContextFactory>(CreateEntityContextFactory, true);
            _namedGraphSelector = new FixedNamedGraphSelector();
        }

        /// <inheritdoc />
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            UrlParser.Register<HttpUrlParser>();
            UrlParser.Register<FtpUrlParser>();
            container.AddFacility<TypedFactoryFacility>();
            var typedFactory = new UrsaCustomTypedFactory();
            InstallRdfDependencies(container, typedFactory);
            InstallRequestPipelineDependencies(container, typedFactory);
            InstallDescriptionDependencies(container, typedFactory);
        }

        private void InstallRequestPipelineDependencies(IWindsorContainer container, UrsaCustomTypedFactory typedFactory)
        {
            var configuration = (HttpConfigurationSection)ConfigurationManager.GetSection(HttpConfigurationSection.ConfigurationSection);
            Type sourceSelectorType = ((configuration != null) && (configuration.DefaultValueRelationSelectorType != null) ?
                configuration.DefaultValueRelationSelectorType :
                typeof(DefaultValueRelationSelector));
            container.Register(Component.For<IConverterProvider>().UsingFactoryMethod((kernel, context) =>
                {
                    var result = new DefaultConverterProvider();
                    result.Initialize(container.ResolveAll<IConverter>);
                    return result;
                }).LifeStyle.PerUniversalWebRequest());
            container.Register(Component.For<IControllerActivator>().UsingFactoryMethod((kernel, context) => new DefaultControllerActivator(UrsaConfigurationSection.InitializeComponentProvider())).LifestyleSingleton());
            container.Register(Component.For<IDefaultValueRelationSelector>().ImplementedBy(sourceSelectorType).LifestyleSingleton());
            container.Register(Component.For<IParameterSourceArgumentBinder>().ImplementedBy<FromQueryStringArgumentBinder>().Activator<NonPublicComponentActivator>().LifestyleSingleton());
            container.Register(Component.For<IParameterSourceArgumentBinder>().ImplementedBy<FromUrlArgumentBinder>().Activator<NonPublicComponentActivator>().LifestyleSingleton());
            container.Register(Component.For<IParameterSourceArgumentBinder>().ImplementedBy<FromBodyArgumentBinder>().Activator<NonPublicComponentActivator>().LifestyleSingleton());
            container.Register(Component.For<IWebRequestProvider>().ImplementedBy<WebRequestProvider>().LifestyleSingleton());
            container.Register(Component.For<IRequestHandler<RequestInfo, ResponseInfo>>().ImplementedBy<RequestHandler>().LifestyleSingleton());
            container.Register(Component.For<IResponseComposer>().ImplementedBy<ResponseComposer>().LifestyleSingleton());
            container.Register(Component.For<IDelegateMapper<RequestInfo>>().ImplementedBy<DelegateMapper>().LifestyleSingleton());
            container.Register(Component.For<IArgumentBinder<RequestInfo>>().ImplementedBy<ArgumentBinder>().LifestyleSingleton());
            container.Register(Component.For<IResultBinder<RequestInfo>>().ImplementedBy<ResultBinder>().LifestyleSingleton());
            container.Register(Component.For<IResponseModelTransformer>().ImplementedBy<RdfPayloadModelTransformer>().Named("RdfPayloadRequestModelTransformer").LifestyleSingleton());
            container.Register(Component.For<IResponseModelTransformer>().ImplementedBy<CollectionResponseModelTransformer>().Named("CollectionResponseModelTransformer").LifestyleSingleton());
            container.Register(Component.For<IRequestModelTransformer>().ImplementedBy<RdfPayloadModelTransformer>().Named("RdfPayloadResponseModelTransformer").LifestyleSingleton());
        }

        private void InstallRdfDependencies(IWindsorContainer container, UrsaCustomTypedFactory typedFactory)
        {
            container.Register(Component.For<ITripleStore>().UsingFactoryMethod(CreateTripleStore).Named("InMemoryTripleStore").LifeStyle.PerUniversalWebRequest());
            container.Register(Component.For<INamedGraphSelector>().Instance(_namedGraphSelector).Named("InMemoryNamedGraphSelector").LifestyleSingleton());
            container.Register(Component.For<IEntityContextFactory>().Instance(_entityContextFactory.Value).Named("InMemoryEntityContextFactory").LifestyleSingleton());
            container.Register(Component.For<IEntityContext>().UsingFactoryMethod(CreateEntityContext).Named("InMemoryEntityContext").LifeStyle.PerUniversalWebRequest());
            container.Register(Component.For<Uri>().Instance(MetaGraphUri).Named("InMemoryMetaGraph").LifestyleSingleton());
            container.Register(Component.For<IEntityContextProvider>().AsFactory(typedFactory).LifeStyle.PerUniversalWebRequest());
        }

        private void InstallDescriptionDependencies(IWindsorContainer container, UrsaCustomTypedFactory typedFactory)
        {
            WindsorComponentProvider componentProvider = container.Resolve<WindsorComponentProvider>();
            container.Register(Component.For(typeof(DescriptionController<>)).Forward<IController>()
                .ImplementedBy(typeof(DescriptionController<>), componentProvider.GenericImplementationMatchingStrategy).LifestyleTransient());
            container.Register(Component.For<EntryPointDescriptionController>().Forward<IController>().ImplementedBy<EntryPointDescriptionController>().LifestyleTransient());
            container.Register(Component.For<ITypeDescriptionBuilder>().ImplementedBy<HydraCompliantTypeDescriptionBuilder>()
                .Named(EntityConverter.Hydra.ToString()).IsDefault().LifestyleSingleton());
            container.Register(Component.For<IServerBehaviorAttributeVisitor>().ImplementedBy<DescriptionBuildingServerBahaviorAttributeVisitor<ParameterInfo>>().Named("Hydra"));
            //// TODO: This should be removed once all API description builders are manually registered.
            ////container.Register(Component.For(typeof(IApiDescriptionBuilder<>)).ImplementedBy(typeof(ApiDescriptionBuilder<>)).LifestyleSingleton().Forward<IApiDescriptionBuilder>());
            container.Register(Component.For<IApiEntryPointDescriptionBuilder>().ImplementedBy<ApiEntryPointDescriptionBuilder>().LifestyleSingleton().Forward<IApiDescriptionBuilder>());
            container.Register(Component.For<IApiDescriptionBuilderFactory>().AsFactory(typedFactory).LifestyleSingleton());
            container.Register(Component.For<IClassGenerator>().ImplementedBy<HydraClassGenerator>().LifestyleSingleton());
            container.Register(Component.For<IUriParser>().ImplementedBy<Web.Http.Description.CodeGen.GenericUriParser>().LifestyleSingleton());
            container.Register(Component.For<IUriParser>().ImplementedBy<HydraUriParser>().LifestyleSingleton().Named(typeof(HydraUriParser).FullName));
            container.Register(Component.For<IUriParser>().ImplementedBy<XsdUriParser>().LifestyleSingleton().Named(typeof(XsdUriParser).FullName));
            container.Register(Component.For<IUriParser>().ImplementedBy<OGuidUriParser>().LifestyleSingleton().Named(typeof(OGuidUriParser).FullName));
            container.Register(Component.For<IXmlDocProvider>().ImplementedBy<XmlDocProvider>().LifestyleSingleton());
        }

        private IEntityContext CreateEntityContext(IKernel kernel, CreationContext context)
        {
            IEntityContext result = null;
            lock (_lock)
            {
                EntityContextFactory entityContextFactory = _entityContextFactory.Value;
                if (!_isBaseUriInitialized)
                {
                    if (kernel.GetAssignableHandlers(typeof(IHttpServerConfiguration)).Any())
                    {
                        var baseUri = kernel.Resolve<IHttpServerConfiguration>().BaseUri;
                        if (baseUri != null)
                        {
                            _isBaseUriInitialized = true;
                            result = CreateThreadSafeEntityContext(
                                entityContextFactory.WithBaseUri(policy => policy.Default.Is(baseUri)),
                                kernel.Resolve<ITripleStore>("InMemoryTripleStore"));
                        }
                    }
                    else
                    {
                        _isBaseUriInitialized = true;
                    }
                }

                if (result == null)
                {
                    result = CreateThreadSafeEntityContext(entityContextFactory, kernel.Resolve<ITripleStore>("InMemoryTripleStore"));
                }
            }

            return result;
        }

        private IEntityContext CreateThreadSafeEntityContext(IEntityContextFactory entityContextFactory, ITripleStore tripleStore)
        {
            var metaGraphUri = tripleStore.Graphs.First().BaseUri;
            var sparqlCommandFactory = (ISparqlCommandFactory)typeof(TripleStoreAdapter).Assembly
                .GetType("RomanticWeb.DotNetRDF.DefaultSparqlCommandFactory")
                .GetConstructor(new[] { typeof(Uri) })
                .Invoke(new object[] { metaGraphUri });
            var sparqlCommandExecutionStrategyFactory = (ISparqlCommandExecutionStrategyFactory)typeof(TripleStoreAdapter).Assembly
                .GetType("RomanticWeb.DotNetRDF.DefaultSparqlCommandExecutionStrategyFactory")
                .GetConstructor(Type.EmptyTypes)
                .Invoke(null);
            var tripleStoreAdapter = new TripleStoreAdapter(tripleStore, sparqlCommandFactory, sparqlCommandExecutionStrategyFactory) { MetaGraphUri = metaGraphUri };
            var result = entityContextFactory.CreateContext();
            result.GetType().GetField("_entitySource", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(result, tripleStoreAdapter);
            return result;
        }

        private ITripleStore CreateTripleStore()
        {
            var store = new ThreadSafeTripleStore();
            var metaGraph = new ThreadSafeGraph() { BaseUri = MetaGraphUri };
            store.Add(metaGraph);
            return store;
        }

        private EntityContextFactory CreateEntityContextFactory()
        {
            return EntityContextFactory
                .FromConfiguration(DescriptionConfigurationSection.Default.DefaultStoreFactoryName)
                .WithDefaultOntologies()
                .WithNamedGraphSelector(_namedGraphSelector)
                .WithDotNetRDF(new TripleStore());
        }
    }
}