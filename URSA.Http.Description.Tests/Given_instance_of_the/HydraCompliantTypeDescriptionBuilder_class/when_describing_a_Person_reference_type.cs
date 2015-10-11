#pragma warning disable 1591
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.CodeGen;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Tests.Data;

namespace Given_instance_of_the.HydraCompliantTypeDescriptionBuilder_class
{
    [TestClass]
    public class when_describing_a_Person_reference_type : HydraCompliantTypeDescriptionBuilderTest<Person>
    {
        [TestMethod]
        public void it_should_create_a_resource_description()
        {
            var context = DescriptionContext.ForType(ApiDocumentation, typeof(Person), Builder);

            var result = context.BuildTypeDescription();
            result = context.TypeDescriptionBuilder.SubClass(context, result);

            result.SingleValue.Should().HaveValue().And.Subject.Value.Should().BeTrue();
            result.Should().BeAssignableTo<IClass>();
        }

        [TestMethod]
        public void it_should_create_a_type_description()
        {
            var result = DescriptionContext.ForType(ApiDocumentation, typeof(Person), Builder).BuildTypeDescription();

            result.Label.Should().Be("Person");
            result.Description.Should().Be(typeof(Person).FullName);
            result.SupportedProperties.Should().HaveCount(4);
        }

        [TestMethod]
        public void it_should_create_an_Id_property_description()
        {
            var result = DescriptionContext.ForType(ApiDocumentation, typeof(Person), Builder).BuildTypeDescription();

            var property = result.SupportedProperties.FirstOrDefault(item => (item.Property != null) && (item.Property.Label == "Key"));

            property.Should().NotBeNull();
            property.Readable.Should().BeTrue();
            property.Writeable.Should().BeTrue();
            property.Required.Should().BeTrue();
            property.Property.Description.Should().Be(typeof(Person) + ".Key");
            property.Property.Domain.Should().Contain(@class => @class.Id.Uri.AbsoluteUri.Contains(typeof(Person).FullName));
            property.Property.Range.Should().HaveCount(1);
            property.Property.Range.First().Should().BeAssignableTo<IClass>();
            ((IClass)property.Property.Range.First()).SubClassOf.Should().Contain(resource => OGuidUriParser.Types.Any(item => item.Value.AbsoluteUri == resource.Id.Uri.AbsoluteUri));
        }

        [TestMethod]
        public void it_should_create_an_FirstName_property_description()
        {
            var result = DescriptionContext.ForType(ApiDocumentation, typeof(Person), Builder).BuildTypeDescription();

            var property = result.SupportedProperties.FirstOrDefault(item => (item.Property != null) && (item.Property.Label == "FirstName"));

            property.Should().NotBeNull();
            property.Readable.Should().BeTrue();
            property.Writeable.Should().BeTrue();
            property.Required.Should().BeFalse();
            property.Property.Description.Should().Be(typeof(Person) + ".FirstName");
            property.Property.Domain.Should().Contain(@class => @class.Id.Uri.AbsoluteUri.Contains(typeof(Person).FullName));
            property.Property.Range.Should().HaveCount(1);
            property.Property.Range.First().Should().BeAssignableTo<IClass>();
            ((IClass)property.Property.Range.First()).SubClassOf.Should().Contain(resource => XsdUriParser.Types.Any(item => item.Value.AbsoluteUri == resource.Id.Uri.AbsoluteUri));
        }

        [TestMethod]
        public void it_should_create_an_LastName_property_description()
        {
            var result = DescriptionContext.ForType(ApiDocumentation, typeof(Person), Builder).BuildTypeDescription();

            var property = result.SupportedProperties.FirstOrDefault(item => (item.Property != null) && (item.Property.Label == "LastName"));

            property.Should().NotBeNull();
            property.Readable.Should().BeTrue();
            property.Writeable.Should().BeTrue();
            property.Required.Should().BeFalse();
            property.Property.Description.Should().Be(typeof(Person) + ".LastName");
            property.Property.Domain.Should().Contain(@class => @class.Id.Uri.AbsoluteUri.Contains(typeof(Person).FullName));
            property.Property.Range.Should().HaveCount(1);
            property.Property.Range.First().Should().BeAssignableTo<IClass>();
            ((IClass)property.Property.Range.First()).SubClassOf.Should().Contain(resource => XsdUriParser.Types.Any(item => item.Value.AbsoluteUri == resource.Id.Uri.AbsoluteUri));
        }

        [TestMethod]
        public void it_should_create_an_Roles_property_description()
        {
            var result = DescriptionContext.ForType(ApiDocumentation, typeof(Person), Builder).BuildTypeDescription();

            var property = result.SupportedProperties.FirstOrDefault(item => (item.Property != null) && (item.Property.Label == "Roles"));

            property.Should().NotBeNull();
            property.Readable.Should().BeTrue();
            property.Writeable.Should().BeTrue();
            property.Required.Should().BeFalse();
            property.Property.Description.Should().Be(typeof(Person) + ".Roles");
            property.Property.Domain.Should().Contain(@class => @class.Id.Uri.AbsoluteUri.Contains(typeof(Person).FullName));
            property.Property.Range.Should().HaveCount(1);
            property.Property.Range.First().Should().BeAssignableTo<IClass>();
            ((IClass)property.Property.Range.First()).SubClassOf.Should().Contain(resource => XsdUriParser.Types.Any(item => item.Value.AbsoluteUri == resource.Id.Uri.AbsoluteUri));
        }
    }
}