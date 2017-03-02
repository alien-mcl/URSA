#pragma warning disable 1591
using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RomanticWeb.Entities;
using RomanticWeb.Vocabularies;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.CodeGen;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.Owl;
using URSA.Web.Http.Description.Tests.Data;
using IClass = URSA.Web.Http.Description.Hydra.IClass;

namespace Given_instance_of_the.HydraCompliantTypeDescriptionBuilder_class
{
    [TestFixture]
    public class when_describing_a_Person_reference_type : HydraCompliantTypeDescriptionBuilderTest<Person>
    {
        [Test]
        public void it_should_create_a_resource_description()
        {
            var context = DescriptionContext.ForType(ApiDocumentation, typeof(Person), Builder);

            var result = context.BuildTypeDescription();
            result = context.TypeDescriptionBuilder.SubClass(context, result);

            result.IsCollection().Should().BeFalse();
            result.Should().BeAssignableTo<IClass>();
        }

        [Test]
        public void it_should_create_a_type_description()
        {
            var result = DescriptionContext.ForType(ApiDocumentation, typeof(Person), Builder).BuildTypeDescription();

            result.Label.Should().Be("Person");
            result.Description.Should().Be(typeof(Person).FullName);
            result.SupportedProperties.Should().HaveCount(4);
        }

        [Test]
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
            OGuidUriParser.Types.Values.Any(iri => iri.AbsoluteUri == ((IClass)property.Property.Range.First()).Id.Uri.AbsoluteUri).Should().BeTrue();
            result.SubClassOf.OfType<IRestriction>().Any(restriction => (restriction.OnProperty.Id.Uri.AbsoluteUri == property.Property.Id.Uri.AbsoluteUri) &&
                (restriction.MaxCardinality == 1)).Should().BeTrue();
        }

        [Test]
        public void it_should_create_a_FirstName_property_description()
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
            XsdUriParser.Types.Values.Any(iri => iri.AbsoluteUri == ((IClass)property.Property.Range.First()).Id.Uri.AbsoluteUri).Should().BeTrue();
            result.SubClassOf.OfType<IRestriction>().Any(restriction => (restriction.OnProperty.Id.Uri.AbsoluteUri == property.Property.Id.Uri.AbsoluteUri) &&
                (restriction.MaxCardinality == 1)).Should().BeTrue();
        }

        [Test]
        public void it_should_create_a_LastName_property_description()
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
            XsdUriParser.Types.Values.Any(iri => iri.AbsoluteUri == ((IClass)property.Property.Range.First()).Id.Uri.AbsoluteUri).Should().BeTrue();
            result.SubClassOf.OfType<IRestriction>().Any(restriction => (restriction.OnProperty.Id.Uri.AbsoluteUri == property.Property.Id.Uri.AbsoluteUri) &&
                (restriction.MaxCardinality == 1)).Should().BeTrue();
        }

        [Test]
        public void it_should_create_a_Roles_property_description()
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
            var range = (IClass)property.Property.Range.First();
            range.SubClassOf.FirstOrDefault(item => item.IsClass(new Uri(EntityConverter.Hydra.AbsoluteUri + "Collection"))).Should().NotBeNull();
            var restriction = range.SubClassOf.Where(item => item.Is(Owl.Restriction)).Cast<IRestriction>().FirstOrDefault();
            restriction.Should().NotBeNull();
            XsdUriParser.Types.Values.Any(iri => iri.AbsoluteUri == restriction.AllValuesFrom.Id.Uri.AbsoluteUri).Should().BeTrue();
            restriction.OnProperty.Id.Uri.AbsoluteUri.Should().Be(EntityConverter.Hydra.AbsoluteUri + "member");
            restriction.MaxCardinality.Should().NotBe(1);
        }
    }
}