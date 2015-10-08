using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Description;
using URSA.Web.Mapping;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class EntryPointControllerDescriptionBuilder_class
    {
        private static readonly Uri EntryPoint = new Uri("/test", UriKind.Relative);
        private EntryPointControllerDescriptionBuilder _builder;

        [TestMethod]
        public void it_should_describe_Add_method_correctly()
        {
            var details = ((IControllerDescriptionBuilder)_builder).BuildDescriptor();

            details.Arguments.Should().ContainKey("entryPoint").WhichValue.Should().Be(EntryPoint);
        }

        [TestInitialize]
        public void Setup()
        {
            Mock<IDefaultValueRelationSelector> defaultSourceSelector = new Mock<IDefaultValueRelationSelector>(MockBehavior.Strict);
            defaultSourceSelector.Setup(instance => instance.ProvideDefault(It.IsAny<ParameterInfo>(), It.IsAny<Verb>()))
                .Returns<ParameterInfo, Verb>((parameter, verb) => FromQueryStringAttribute.For(parameter));
            defaultSourceSelector.Setup(instance => instance.ProvideDefault(It.IsAny<ParameterInfo>()))
                .Returns<ParameterInfo>(parameter => new ToBodyAttribute());
            _builder = new EntryPointControllerDescriptionBuilder(EntryPoint, defaultSourceSelector.Object);
        }

        [TestCleanup]
        public void Teardown()
        {
            _builder = null;
        }
    }
}