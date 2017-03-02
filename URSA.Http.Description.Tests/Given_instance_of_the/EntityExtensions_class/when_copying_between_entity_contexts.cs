using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RomanticWeb.Entities;
using URSA.Web.Http.Description.Entities;

namespace Given_instance_of_the.EntityExtensions_class
{
    [TestFixture]
    public class when_copying_between_entity_contexts : EntityExtensionsTest
    {
        [Test]
        public void it_should_copy_an_entity()
        {
            var expectedId = new EntityId("http://temp.uri/copy/");

            TargetInstance = TargetEntityContext.Copy(SourceInstance, expectedId);

            TargetInstance.Id.Should().Be(expectedId);
            TargetInstance.Name.Should().Be("Test");
        }

        [Test]
        public void it_should_copy_nested_entity()
        {
            var expectedId = new EntityId("http://temp.uri/copy/");

            TargetInstance = TargetEntityContext.Copy(SourceInstance, expectedId);

            TargetInstance.RelatedProduct.Should().NotBeNull();
            TargetInstance.Similar.Should().HaveCount(1);
        }

        [Test]
        public void it_should_copy_underlying_RDF_statements()
        {
            var expectedId = new EntityId("http://temp.uri/copy/");

            TargetInstance = TargetEntityContext.Copy(SourceInstance, expectedId);
            TargetEntityContext.Commit();

            TargetStore.Triples.Should().HaveCount(SourceStore.Triples.Count());
        }
    }
}