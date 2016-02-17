using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RomanticWeb;
using RomanticWeb.Mapping;
using RomanticWeb.Mapping.Attributes;
using RomanticWeb.Mapping.Model;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description.CodeGen;
using URSA.Web.Http.Testing;
using URSA.Web.Http.Tests.Data;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ExpressionTreeConverter_class : ConverterTest<ExpressionTreeConverter, Expression<Func<IProduct, bool>>>
    {
        private const string EntityBody = "<http://temp.uri/vocab#price> eq 0";
        private static readonly Expression<Func<IProduct, bool>> Entity = t => t.Price == 0.0;

        protected override Expression<Func<IProduct, bool>> SingleEntity { get { return Entity; } }

        protected override string SingleEntityBody { get { return EntityBody; } }

        protected override Expression<Func<IProduct, bool>>[] MultipleEntities { get { return null; } }

        [TestMethod]
        public override void it_should_deserialize_message_body_as_an_array_of_entities()
        {
        }

        [TestMethod]
        public override void it_should_deserialize_message_as_an_array_of_entities()
        {
        }

        [TestMethod]
        public override void it_should_serialize_an_entity_to_message()
        {
        }

        [TestMethod]
        public override void it_should_serialize_array_of_entities_to_message()
        {
        }

        [TestMethod]
        public override void it_should_throw_when_no_given_type_is_provided_for_deserialization()
        {
        }

        [TestMethod]
        public override void it_should_throw_when_no_response_is_provided_for_serialization()
        {
        }

        [TestMethod]
        public override void it_should_throw_when_no_response_is_provided_for_serialization_compatibility_test()
        {
        }

        [TestMethod]
        public override void it_should_throw_when_no_given_type_is_provided_for_serialization_compatibility_test()
        {
        }

        [TestMethod]
        public override void it_should_test_serialization_compatibility()
        {
        }

        protected override void AssertSingleEntity(Expression<Func<IProduct, bool>> result)
        {
            result.ShouldBeEquivalentTo(SingleEntity);
        }

        protected override ExpressionTreeConverter CreateInstance()
        {
            Func<PropertyInfo, Uri, bool> attributeMatch = (property, uri) =>
                (from attribute in property.GetCustomAttributes<PropertyAttribute>(true)
                let propertyMapping = attribute as PropertyAttribute
                where AbsoluteUriComparer.Default.Equals(propertyMapping.Uri, uri)
                select attribute).Any();
            Func<Uri, string> propertyMatch = uri => typeof(IProduct).GetProperties().First(property => attributeMatch(property, uri)).Name;
            var mappingsRepository = new Mock<IMappingsRepository>(MockBehavior.Strict);
            mappingsRepository.Setup(instance => instance.MappingForProperty(It.IsAny<Uri>()))
                .Returns<Uri>(uri =>
                    {
                        var result = new Mock<IPropertyMapping>(MockBehavior.Strict);
                        result.SetupGet(instance => instance.Name).Returns(propertyMatch(uri));
                        return result.Object;
                    });
            var entityContextFactory = new Mock<IEntityContextFactory>(MockBehavior.Strict);
            entityContextFactory.SetupGet(instance => instance.Mappings).Returns(mappingsRepository.Object);
            return new ExpressionTreeConverter(new IUriParser[0], entityContextFactory.Object);
        }
    }
}