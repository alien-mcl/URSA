using System;
using System.Linq;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using RomanticWeb;
using RomanticWeb.Configuration;
using RomanticWeb.DotNetRDF;
using RomanticWeb.DotNetRDF.Configuration;
using URSA.Example.WebApplication.Data;
using URSA.Web.Http.Configuration;
using VDS.RDF;

namespace URSA.Example.WebApplication
{
    /// <summary>Installs HTTP components.</summary>
    public class Installer : IWindsorInstaller
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
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            InstallRdfDependencies(container);
            var jsonFileRepository = new JsonFilePersistingRepository<Person, Guid>(AppDomain.CurrentDomain.GetApplicationStoragePath());
            container.Register(Component.For<IPersistingRepository<Person, Guid>>().Instance(jsonFileRepository).Named("PersonsJsonFileRepository"));
        }

        private void InstallRdfDependencies(IWindsorContainer container)
        {
            container.Register(Component.For<ITripleStore>().Instance(_tripleStore.Value).LifestyleSingleton());
            container.Register(Component.For<IEntityContextFactory>().Instance(_entityContextFactory.Value).LifestyleSingleton());
            container.Register(Component.For<IEntityContext>().UsingFactoryMethod(CreateEntityContext).LifeStyle.HybridPerWebRequestPerThread());
        }

        private IEntityContext CreateEntityContext(IKernel kernel, CreationContext context)
        {
            IEntityContext result = null;
            lock (_lock)
            {
                EntityContextFactory entityContextFactory = _entityContextFactory.Value;
                if (!_isBaseUriInitialized)
                {
                    var baseUri = kernel.Resolve<IHttpServerConfiguration>().BaseUri;
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
            var metaGraph = store.Graphs.FirstOrDefault(item => AbsoluteUriComparer.Default.Equals(item.BaseUri, ConfigurationSectionHandler.Default.Factories["file"].MetaGraphUri));
            if (metaGraph == null)
            {
                store.Add(new ThreadSafeGraph() { BaseUri = ConfigurationSectionHandler.Default.Factories["file"].MetaGraphUri });
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