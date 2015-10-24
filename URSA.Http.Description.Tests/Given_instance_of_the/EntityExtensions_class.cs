using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RomanticWeb;
using RomanticWeb.Configuration;
using RomanticWeb.DotNetRDF;
using RomanticWeb.Entities;
using URSA.Configuration;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.Tests.Data;
using VDS.RDF;

namespace Given_instance_of_the
{
    [TestClass]
    public class EntityExtensions_class
    {
        private ITripleStore _store;
        private IEntityContext _entityContext;
        private IProduct _instance;

        [TestMethod]
        public void it_should_rename_an_entity()
        {
            Uri expected = new Uri(_instance.Id.Uri + (_instance.Key = Guid.Empty).ToString());

            var result = _instance.Rename(new EntityId(expected));

            _store.Graphs.Any(graph => AbsoluteUriComparer.Default.Equals(graph.BaseUri, expected)).Should().BeTrue();
            result.Name.Should().Be(_instance.Name);
            result.Key.Should().Be(Guid.Empty);
            result.Id.Uri.AbsoluteUri.Should().Be(expected.AbsoluteUri);
        }

        [TestInitialize]
        public void Setup()
        {
            var metaGraphUri = ConfigurationSectionHandler.Default.Factories[DescriptionConfigurationSection.Default.DefaultStoreFactoryName].MetaGraphUri;
            _store = new TripleStore();
            _store.Add(new Graph() { BaseUri = metaGraphUri });
            _entityContext = new EntityContextFactory()
                .WithMetaGraphUri(metaGraphUri)
                .WithDefaultOntologies()
                .WithMappings(load => load.FromAssemblyOf<IProduct>())
                .WithDotNetRDF(_store)
                .CreateContext();
            _instance = _entityContext.Create<IProduct>(new EntityId("http://temp.uri/product/"));
            _instance.Name = "Test";
            _entityContext.Commit();
        }
    }
}