using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the.ControllerDescriptionBuilder_class
{
    [TestClass]
    public class when_having_a_CRUD_controller
    {
        private ControllerDescriptionBuilder<CrudController> _builder;

        [TestMethod]
        public void it_should_build_a_hierarchical_path()
        {
            var method = typeof(CrudController).GetMethod("SetRoles");
            var details = _builder.BuildDescriptor().Operations.Cast<OperationInfo<Verb>>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.UriTemplate.Should().Be("/api/person/{id}/roles");
            details.ProtocolSpecificCommand.Should().Be(Verb.POST);
        }

        [TestInitialize]
        public void Setup()
        {
            Mock<IDefaultValueRelationSelector> defaultSourceSelector = new Mock<IDefaultValueRelationSelector>(MockBehavior.Strict);
            defaultSourceSelector.Setup(instance => instance.ProvideDefault(It.IsAny<ParameterInfo>(), It.IsAny<Verb>()))
                .Returns<ParameterInfo, Verb>((parameter, verb) =>
                    (parameter.ParameterType == typeof(int) ? (ParameterSourceAttribute)FromUriAttribute.For(parameter) : FromBodyAttribute.For(parameter)));
            defaultSourceSelector.Setup(instance => instance.ProvideDefault(It.IsAny<ParameterInfo>()))
                .Returns<ParameterInfo>(parameter => new ToBodyAttribute());
            _builder = new ControllerDescriptionBuilder<CrudController>(defaultSourceSelector.Object);
        }

        [TestCleanup]
        public void Teardown()
        {
            _builder = null;
        }
    }
}