using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the.ControllerDescriptionBuilder_class
{
    [TestFixture]
    public class when_having_a_CRUD_controller
    {
        private ControllerDescriptionBuilder<CrudController> _builder;

        [Test]
        public void it_should_build_a_hierarchical_path()
        {
            var method = typeof(CrudController).GetMethod("SetRoles");
            var details = _builder.BuildDescriptor().Operations.Cast<OperationInfo<Verb>>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.UrlTemplate.Should().Be("/api/person/{id}/roles");
            details.ProtocolSpecificCommand.Should().Be(Verb.POST);
        }

        [SetUp]
        public void Setup()
        {
            Mock<IDefaultValueRelationSelector> defaultSourceSelector = new Mock<IDefaultValueRelationSelector>(MockBehavior.Strict);
            defaultSourceSelector.Setup(instance => instance.ProvideDefault(It.IsAny<ParameterInfo>(), It.IsAny<Verb>()))
                .Returns<ParameterInfo, Verb>((parameter, verb) =>
                    (parameter.ParameterType == typeof(int) ? (ParameterSourceAttribute)FromUrlAttribute.For(parameter) : FromBodyAttribute.For(parameter)));
            defaultSourceSelector.Setup(instance => instance.ProvideDefault(It.IsAny<ParameterInfo>()))
                .Returns<ParameterInfo>(parameter => new ToBodyAttribute());
            _builder = new ControllerDescriptionBuilder<CrudController>(defaultSourceSelector.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _builder = null;
        }
    }
}