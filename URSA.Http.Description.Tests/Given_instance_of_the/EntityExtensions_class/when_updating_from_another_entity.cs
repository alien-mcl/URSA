using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RomanticWeb.Entities;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.Tests.Data;

namespace Given_instance_of_the.EntityExtensions_class
{
    [TestClass]
    public class when_updating_from_another_entity : EntityExtensionsTest
    {
        private static readonly EntityId ExpectedId = new EntityId("http://temp.uri/test/");

        [TestMethod]
        public void it_should_update_an_entity()
        {
            TargetInstance = TargetInstance.Update(SourceInstance);

            TargetInstance.Id.Should().Be(ExpectedId);
            TargetInstance.Name.Should().Be("Test");
        }

        [TestMethod]
        public void it_should_update_nested_entity()
        {
            TargetInstance = TargetInstance.Update(SourceInstance);

            TargetInstance.RelatedProduct.Should().NotBeNull();
            TargetInstance.Similar.Should().HaveCount(1);
        }

        [TestMethod]
        public void it_should_update_underlying_RDF_statements()
        {
            TargetInstance = TargetInstance.Update(SourceInstance);
            TargetEntityContext.Commit();

            TargetStore.Triples.Should().HaveCount(SourceStore.Triples.Count());
        }

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
            TargetInstance = TargetEntityContext.Create<IProduct>(ExpectedId);
        }
    }
}