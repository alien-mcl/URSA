using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using URSA;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Description;
using URSA.Web.Mapping;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class EntryPointControllerDescriptionBuilder_class
    {
        private static readonly HttpUrl EntryPoint = (HttpUrl)UrlParser.Parse("/test");
        private EntryPointControllerDescriptionBuilder _builder;

        [Test]
        public void it_should_describe_Add_method_correctly()
        {
            var details = ((IControllerDescriptionBuilder)_builder).BuildDescriptor();

            details.Arguments.Should().ContainKey("entryPoint").WhichValue.Should().Be(EntryPoint);
        }

        [SetUp]
        public void Setup()
        {
            Mock<IDefaultValueRelationSelector> defaultSourceSelector = new Mock<IDefaultValueRelationSelector>(MockBehavior.Strict);
            defaultSourceSelector.Setup(instance => instance.ProvideDefault(It.IsAny<ParameterInfo>(), It.IsAny<Verb>()))
                .Returns<ParameterInfo, Verb>((parameter, verb) => FromQueryStringAttribute.For(parameter));
            defaultSourceSelector.Setup(instance => instance.ProvideDefault(It.IsAny<ParameterInfo>()))
                .Returns<ParameterInfo>(parameter => new ToBodyAttribute());
            _builder = new EntryPointControllerDescriptionBuilder(EntryPoint, defaultSourceSelector.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _builder = null;
        }
    }
}