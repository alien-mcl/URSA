using RomanticWeb;
using RomanticWeb.DotNetRDF;
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Autofac;
using RomanticWeb.Configuration;
using RomanticWeb.NamedGraphs;
using URSA.CodeGen;
using URSA.Configuration;
using URSA.Http.AutoFac.Description;
using URSA.Http.AutoFac.Entities;
using URSA.Web;
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
using GenericUriParser = URSA.Web.Http.Description.CodeGen.GenericUriParser;

namespace URSA.AutoFac
{
    /// <summary>Installs HTTP components.</summary>
    public class HttpInstaller : Autofac.Module
    {
        private static readonly Uri MetaGraphUri = ConfigurationSectionHandler.Default.Factories.Cast<FactoryElement>()
            .First(factory => factory.Name == DescriptionConfigurationSection.Default.DefaultStoreFactoryName).MetaGraphUri;
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
        protected override void Load(ContainerBuilder builder)
        {
            UrlParser.Register<HttpUrlParser>();
            UrlParser.Register<FtpUrlParser>();
            InstallRdfDependencies(builder);
            InstallRequestPipelineDependencies(builder);
            InstallDescriptionDependencies(builder);
        }

        private void InstallRequestPipelineDependencies(ContainerBuilder builder)
        {
            var configuration = (HttpConfigurationSection)ConfigurationManager.GetSection(HttpConfigurationSection.ConfigurationSection);
            Type sourceSelectorType = ((configuration != null) && (configuration.DefaultValueRelationSelectorType != null) ?
                configuration.DefaultValueRelationSelectorType :
                typeof(DefaultValueRelationSelector));
            builder.RegisterType<DelegateMapper>().As<IDelegateMapper<RequestInfo>>().InstancePerLifetimeScope();
            builder.RegisterType(sourceSelectorType).As<IDefaultValueRelationSelector>().SingleInstance();
            builder.RegisterType<FromQueryStringArgumentBinder>().As<IParameterSourceArgumentBinder>()
                .FindConstructorsWith(type => type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)).InstancePerLifetimeScope();
            builder.RegisterType<FromUrlArgumentBinder>().As<IParameterSourceArgumentBinder>()
                .FindConstructorsWith(type => type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)).InstancePerLifetimeScope();
            builder.RegisterType<FromBodyArgumentBinder>().As<IParameterSourceArgumentBinder>()
                .FindConstructorsWith(type => type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)).InstancePerLifetimeScope();
            builder.RegisterType<WebRequestProvider>().As<IWebRequestProvider>().SingleInstance();
            builder.RegisterType<RequestHandler>().As<IRequestHandler<RequestInfo, ResponseInfo>>().InstancePerHttpRequest();
            builder.RegisterType<ResponseComposer>().As<IResponseComposer>().InstancePerLifetimeScope();
            builder.RegisterType<ArgumentBinder>().As<IArgumentBinder<RequestInfo>>().InstancePerLifetimeScope();
            builder.RegisterType<ResultBinder>().As<IResultBinder<RequestInfo>>().InstancePerLifetimeScope();
            builder.RegisterType<RdfPayloadModelTransformer>().As<IResponseModelTransformer>()
                .Named<IResponseModelTransformer>("RdfPayloadResponseModelTransformer").InstancePerHttpRequest();
            builder.RegisterType<CollectionResponseModelTransformer>().As<IResponseModelTransformer>()
                .Named<IResponseModelTransformer>("CollectionResponseModelTransformer").InstancePerHttpRequest();
            builder.RegisterType<RdfPayloadModelTransformer>().As<IRequestModelTransformer>()
                .Named<IRequestModelTransformer>("RdfPayloadRequestModelTransformer").InstancePerHttpRequest();
        }

        private void InstallRdfDependencies(ContainerBuilder builder)
        {
            builder.RegisterInstance(MetaGraphUri).Named<Uri>("InMemoryMetaGraph").SingleInstance();
            builder.RegisterInstance(_namedGraphSelector).As<INamedGraphSelector>().SingleInstance();
            builder.RegisterInstance(_entityContextFactory.Value).Named<IEntityContextFactory>("InMemoryEntityContextFactory").SingleInstance();
            builder.Register(CreateTripleStore).Named<ITripleStore>("InMemoryTripleStore").InstancePerHttpRequest();
            builder.Register(CreateEntityContext).Named<IEntityContext>("InMemoryEntityContext").InstancePerHttpRequest();
            builder.Register(context => new DefaultEntityContextProvider(
                context.ResolveNamed<IEntityContext>("InMemoryEntityContext"),
                context.ResolveNamed<ITripleStore>("InMemoryTripleStore"),
                context.ResolveNamed<Uri>("InMemoryMetaGraph"))).As<IEntityContextProvider>().InstancePerHttpRequest();
        }
        
        private void InstallDescriptionDependencies(ContainerBuilder builder)
        {
            builder.RegisterType(typeof(DescriptionController<>)).As(typeof(DescriptionController<>)).As<IController>().InstancePerDependency();
            builder.RegisterType<EntryPointDescriptionController>().As(typeof(EntryPointDescriptionController)).As<IController>().InstancePerDependency();
            builder.RegisterType<HydraCompliantTypeDescriptionBuilder>().As<ITypeDescriptionBuilder>()
                .Named<ITypeDescriptionBuilder>(EntityConverter.Hydra.ToString()).SingleInstance();
            builder.RegisterType<DescriptionBuildingServerBahaviorAttributeVisitor<ParameterInfo>>().As<IServerBehaviorAttributeVisitor>()
                .Named<IServerBehaviorAttributeVisitor>("Hydra");
            //// TODO: This should be removed once all API description builders are manually registered.
            //// builder.RegisterType(typeof(ApiDescriptionBuilder<>)).AsSelf().As<IApiDescriptionBuilder>().SingleInstance();
            builder.RegisterType<ApiEntryPointDescriptionBuilder>().As<IApiEntryPointDescriptionBuilder>().As<IApiDescriptionBuilder>().SingleInstance();
            builder.Register(context =>
                {
                    var childContext = context.Resolve<IComponentContext>();
                    return new DefaultApiDescriptionBuilderFactory(
                        type => (IApiDescriptionBuilder)childContext.Resolve(typeof(IApiDescriptionBuilder<>).MakeGenericType(type)));
                })
                .As<IApiDescriptionBuilderFactory>().SingleInstance();
            builder.RegisterType<HydraClassGenerator>().As<IClassGenerator>().SingleInstance();
            builder.RegisterType<GenericUriParser>().As<IUriParser>().SingleInstance();
            builder.RegisterType<HydraUriParser>().As<IUriParser>().Named<IUriParser>(typeof(HydraUriParser).FullName).SingleInstance();
            builder.RegisterType<XsdUriParser>().As<IUriParser>().Named<IUriParser>(typeof(XsdUriParser).FullName).SingleInstance();
            builder.RegisterType<OGuidUriParser>().As<IUriParser>().Named<IUriParser>(typeof(OGuidUriParser).FullName).SingleInstance();
            builder.RegisterType<XmlDocProvider>().As<IXmlDocProvider>().SingleInstance();
        }

        private IEntityContext CreateEntityContext(IComponentContext context)
        {
            IEntityContext result = null;
            lock (_lock)
            {
                EntityContextFactory entityContextFactory = _entityContextFactory.Value;
                if (!_isBaseUriInitialized)
                {
                    if (context.IsRegistered<IHttpServerConfiguration>())
                    {
                        var baseUri = context.Resolve<IHttpServerConfiguration>().BaseUri;
                        if (baseUri != null)
                        {
                            _isBaseUriInitialized = true;
                            result = CreateThreadSafeEntityContext(
                                entityContextFactory.WithBaseUri(policy => policy.Default.Is(baseUri)),
                                context.ResolveNamed<ITripleStore>("InMemoryTripleStore"));
                        }
                    }
                    else
                    {
                        _isBaseUriInitialized = true;
                    }
                }

                if (result == null)
                {
                    result = CreateThreadSafeEntityContext(entityContextFactory, context.ResolveNamed<ITripleStore>("InMemoryTripleStore"));
                }
            }

            return result;
        }

        private IEntityContext CreateThreadSafeEntityContext(IEntityContextFactory entityContextFactory, ITripleStore tripleStore)
        {
            var metaGraphUri = tripleStore.Graphs.First().BaseUri;
            var sparqlCommandFactory = (ISparqlCommandFactory)typeof(TripleStoreAdapter).GetTypeInfo().Assembly
                .GetType("RomanticWeb.DotNetRDF.DefaultSparqlCommandFactory")
                .GetConstructor(new[] { typeof(Uri) })
                .Invoke(new object[] { metaGraphUri });
            var sparqlCommandExecutionStrategyFactory = (ISparqlCommandExecutionStrategyFactory)typeof(TripleStoreAdapter).GetTypeInfo().Assembly
                .GetType("RomanticWeb.DotNetRDF.DefaultSparqlCommandExecutionStrategyFactory")
                .GetConstructor(Type.EmptyTypes)
                .Invoke(null);
            var tripleStoreAdapter = new TripleStoreAdapter(tripleStore, sparqlCommandFactory, sparqlCommandExecutionStrategyFactory) { MetaGraphUri = metaGraphUri };
            var result = entityContextFactory.CreateContext();
            result.GetType().GetField("_entitySource", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(result, tripleStoreAdapter);
            return result;
        }

        private ITripleStore CreateTripleStore(IComponentContext context)
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