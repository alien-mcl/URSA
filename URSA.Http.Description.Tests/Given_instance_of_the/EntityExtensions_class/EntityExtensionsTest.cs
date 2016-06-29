using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RomanticWeb;
using RomanticWeb.Configuration;
using RomanticWeb.DotNetRDF;
using RomanticWeb.Entities;
using URSA.Configuration;
using URSA.Web.Http.Description.Tests.Data;
using VDS.RDF;

namespace Given_instance_of_the.EntityExtensions_class
{
    [TestClass]
    public abstract class EntityExtensionsTest
    {
        protected ITripleStore SourceStore { get; private set; }

        protected ITripleStore TargetStore { get; private set; }

        protected IEntityContext SourceEntityContext { get; private set; }

        protected IEntityContext TargetEntityContext { get; private set; }

        protected IProduct SourceInstance { get; private set; }

        protected IProduct TargetInstance { get; set; }

        [TestInitialize]
        public virtual void Setup()
        {
            var metaGraphUri = ConfigurationSectionHandler.Default.Factories[DescriptionConfigurationSection.Default.DefaultStoreFactoryName].MetaGraphUri;
            ITripleStore store;
            SourceEntityContext = SetupEntityContext(metaGraphUri, out store);
            SourceStore = store;
            TargetEntityContext = SetupEntityContext(metaGraphUri, out store);
            TargetStore = store;
            SourceInstance = SourceEntityContext.Create<IProduct>(new EntityId("http://temp.uri/product/"));
            SourceInstance.Name = "Test";
            SourceInstance.RelatedProduct = SourceEntityContext.Create<IProduct>(new EntityId("http://temp.uri/related-product/"));
            SourceInstance.Similar.Add(SourceEntityContext.Create<IProduct>(new EntityId("http://temp.uri/similar-product/")));
            SourceEntityContext.Commit();
        }

        private IEntityContext SetupEntityContext(Uri metaGraphUri, out ITripleStore store)
        {
            store = new TripleStore();
            store.Add(new Graph() { BaseUri = metaGraphUri });
            return new EntityContextFactory()
                .WithMetaGraphUri(metaGraphUri)
                .WithDefaultOntologies()
                .WithMappings(load => load.FromAssemblyOf<IProduct>())
                .WithDotNetRDF(store)
                .CreateContext();
        }
    }
}