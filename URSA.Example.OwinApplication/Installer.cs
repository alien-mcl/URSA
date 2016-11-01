using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using RomanticWeb;
using RomanticWeb.Configuration;
using RomanticWeb.DotNetRDF;
using RomanticWeb.DotNetRDF.Configuration;
using URSA.AutoFac;
using URSA.Example.WebApplication.Data;
using URSA.Web.Http.Configuration;
using VDS.RDF;
using Module = Autofac.Module;

namespace URSA.Example.OwinApplication
{
    /// <summary>Installs HTTP components.</summary>
    public class Installer : Module
    {
        private readonly Lazy<ITripleStore> _tripleStore; 
        private readonly Lazy<EntityContextFactory> _entityContextFactory;
        private readonly object _lock = new object();
        private bool _isBaseUriInitialized;

        /// <summary>Initializes a new instance of the <see cref="Installer" /> class.</summary>
        public Installer()
        {
            _tripleStore = new Lazy<ITripleStore>(CreateTripleStore, true);
            _entityContextFactory = new Lazy<EntityContextFactory>(CreateEntityContextFactory, true);
        }

        /// <inheritdoc />
        protected override void Load(ContainerBuilder builder)
        {
            InstallRdfDependencies(builder);
#if CORE
            var storagePath = Path.Combine(Assembly.GetEntryAssembly().Location, "..", "App_Data");
#else
            var storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "App_Data");
#endif
            var jsonFileRepository = new JsonFilePersistingRepository<Person, Guid>(storagePath);
            builder.RegisterInstance(jsonFileRepository).Named<IPersistingRepository<Person, Guid>>("PersonsJsonFileRepository");
        }

        private void InstallRdfDependencies(ContainerBuilder builder)
        {
            builder.RegisterInstance(_tripleStore.Value).As<ITripleStore>().SingleInstance();
            builder.RegisterInstance(_entityContextFactory.Value).As<IEntityContextFactory>().SingleInstance();
            builder.Register(CreateEntityContext).As<IEntityContext>().InstancePerHttpRequest();
        }

        private IEntityContext CreateEntityContext(IComponentContext context)
        {
            IEntityContext result = null;
            lock (_lock)
            {
                EntityContextFactory entityContextFactory = _entityContextFactory.Value;
                if (!_isBaseUriInitialized)
                {
                    var baseUri = context.Resolve<IHttpServerConfiguration>().BaseUri;
                    if (baseUri != null)
                    {
                        _isBaseUriInitialized = true;
                        result = entityContextFactory.WithBaseUri(policy => policy.Default.Is(baseUri)).CreateContext();
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
            var store = StoresConfigurationSection.Default.CreateStore("file");
            var metaGraph = store.Graphs.FirstOrDefault(item =>
                AbsoluteUriComparer.Default.Equals(item.BaseUri, ConfigurationSectionHandler.Default.Factories.Cast<FactoryElement>().First(factory => factory.Name == "file").MetaGraphUri));
            if (metaGraph == null)
            {
                store.Add(new ThreadSafeGraph() { BaseUri = ConfigurationSectionHandler.Default.Factories.Cast<FactoryElement>().First(factory => factory.Name == "file").MetaGraphUri });
            }

            return store;
        }

        private EntityContextFactory CreateEntityContextFactory()
        {
            return EntityContextFactory
                .FromConfiguration("file")
                .WithDefaultOntologies()
                .WithDotNetRDF(_tripleStore.Value);
        }
    }
}