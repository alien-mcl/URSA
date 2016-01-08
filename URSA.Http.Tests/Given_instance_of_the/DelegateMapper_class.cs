using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using URSA.Security;
using URSA.Web;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Description;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class DelegateMapper_class
    {
        private IController _controller;
        private OperationInfo _operation; 
        private IDelegateMapper _delegateMapper;

        [TestMethod]
        public void it_should_map_request()
        {
            var result = _delegateMapper.MapRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test/add?operandA=1&operandB=2"), new MemoryStream(), new BasicClaimBasedIdentity()));

            result.Should().NotBeNull();
            result.Target.Should().Be(_controller);
            result.Operation.Should().Be(_operation);
            result.MethodRoute.Should().Be(_operation.Uri);
        }

        [TestMethod]
        public void it_should_map_OPTIONS_request()
        {
            var result = _delegateMapper.MapRequest(new RequestInfo(Verb.OPTIONS, new Uri("http://temp.uri/api/test/add?operandA=1&operandB=2"), new MemoryStream(), new BasicClaimBasedIdentity()));

            result.Should().NotBeNull();
            result.Target.Should().BeOfType<OptionsController>();
            ((OperationInfo<Verb>)result.Operation).ProtocolSpecificCommand.Should().Be(Verb.OPTIONS);
            result.MethodRoute.Should().Be(_operation.Uri);
        }

        [TestInitialize]
        public void Setup()
        {
            var method = typeof(TestController).GetMethod("Add");
            _controller = new TestController();
            _operation = new OperationInfo<Verb>(
                method,
                new Uri("add", UriKind.Relative),
                "add?{?operandA}&{?operandB}",
                new Regex(".*"),
                Verb.GET,
                method.GetParameters().Select(parameter => (ValueInfo)new ArgumentInfo(parameter, FromQueryStringAttribute.For(parameter), "add?{?" + parameter.Name + "}", parameter.Name)).ToArray());
            var controllerInfo = new ControllerInfo<TestController>(null, new Uri("api/test", UriKind.Relative), _operation);
            Mock<IHttpControllerDescriptionBuilder> controllerDescriptionBuilder = new Mock<IHttpControllerDescriptionBuilder>(MockBehavior.Strict);
            controllerDescriptionBuilder.Setup(instance => instance.BuildDescriptor()).Returns(controllerInfo);
            Mock<IControllerActivator> controllerActivator = new Mock<IControllerActivator>(MockBehavior.Strict);
            controllerActivator.Setup(instance => instance.CreateInstance(It.IsAny<Type>(), It.IsAny<IDictionary<string, object>>())).Returns(_controller);
            _delegateMapper = new DelegateMapper(new[] { controllerDescriptionBuilder.Object }, controllerActivator.Object);
        }

        [TestCleanup]
        public void Teardown()
        {
            _controller = null;
            _operation = null;
            _delegateMapper = null;
        }
    }
}