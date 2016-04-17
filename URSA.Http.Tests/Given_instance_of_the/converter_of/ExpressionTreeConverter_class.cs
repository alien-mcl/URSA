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
        private static readonly string PropertyUri = "javascript:Person.Key";

        private Mock<IUriParser> _uriParser;

        protected override Expression<Func<IProduct, bool>> SingleEntity { get { return Entity; } }

        protected override string SingleEntityBody { get { return EntityBody; } }

        protected override Expression<Func<IProduct, bool>>[] MultipleEntities { get { return null; } }

        protected override bool SupportsMultipleInstances { get { return false; } }

        protected override bool SupportsSerialization { get { return false; } }

        [TestMethod]
        public virtual void it_should_deserialize_query_as_an_to_an_expression_tree()
        {
            Expression<Func<Person, bool>> expected = t => t.Key == 0;

            var result = ConvertTo<Expression<Func<Person, bool>>>("POST", OperationName, SingleEntityContentType, "<" + PropertyUri + "> eq 0");

            result.ShouldBeEquivalentTo(expected);
        }

        protected override void AssertSingleEntity(Expression<Func<IProduct, bool>> result)
        {
            result.ShouldBeEquivalentTo(SingleEntity);
        }

        protected override ExpressionTreeConverter CreateInstance()
        {
            Func<PropertyInfo, Uri, bool> attributeMatch = (property, uri) =>
                (from attribute in property.GetCustomAttributes<PropertyAttribute>(true)
                where AbsoluteUriComparer.Default.Equals(attribute.Uri, uri)
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
            string @namespace;
            _uriParser = new Mock<IUriParser>(MockBehavior.Strict);
            _uriParser.Setup(instance => instance.IsApplicable(It.Is<Uri>(uri => uri.ToString() == PropertyUri))).Returns(UriParserCompatibility.ExactMatch);
            _uriParser.Setup(instance => instance.Parse(It.Is<Uri>(uri => uri.ToString() == PropertyUri), out @namespace)).Returns("Key");
            return new ExpressionTreeConverter(new[] { _uriParser.Object }, entityContextFactory.Object);
        }
    }
}