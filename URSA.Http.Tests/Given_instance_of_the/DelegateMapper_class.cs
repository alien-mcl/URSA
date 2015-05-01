using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using URSA.Web;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class DelegateMapper_class
    {
        private RequestInfo _request;
        private IController _controller;
        private OperationInfo _operation; 
        private IDelegateMapper _delegateMapper;

        [TestMethod]
        public void it_should_map_request()
        {
            var result = _delegateMapper.MapRequest(_request);

            result.Should().NotBeNull();
            result.Target.Should().Be(_controller);
            result.Operation.Should().Be(_operation);
            result.MethodRoute.Should().Be(_operation.Uri);
        }

        [TestInitialize]
        public void Setup()
        {
            var method = typeof(TestController).GetMethod("Add");
            _request = new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test/add?operandA=1&operandB=2"), new MemoryStream());
            _controller = new TestController();
            _operation = new OperationInfo<RequestInfo>(
                method,
                new Uri("add", UriKind.Relative),
                "add?{?operandA}&{?operandB}",
                new Regex("add?operandA=[^&]+?&operandB=[^&]"),
                _request,
                method.GetParameters().Select(parameter => (ValueInfo)new ArgumentInfo(parameter, FromQueryStringAttribute.For(parameter), "add?{?" + parameter.Name + "}", parameter.Name)).ToArray());
            var controllerInfo = new ControllerInfo<TestController>(new Uri("api/test", UriKind.Relative), _operation);
            Mock<IHttpControllerDescriptionBuilder> controllerDescriptionBuilder = new Mock<IHttpControllerDescriptionBuilder>(MockBehavior.Strict);
            controllerDescriptionBuilder.Setup(instance => instance.BuildDescriptor()).Returns(controllerInfo);
            Mock<IControllerActivator> controllerActivator = new Mock<IControllerActivator>(MockBehavior.Strict);
            controllerActivator.Setup(instance => instance.CreateInstance(It.IsAny<Type>())).Returns(_controller);
            _delegateMapper = (IDelegateMapper)new DelegateMapper(new[] { controllerDescriptionBuilder.Object }, controllerActivator.Object);
        }

        [TestCleanup]
        public void Teardown()
        {
            _request = null;
            _controller = null;
            _operation = null;
            _delegateMapper = null;
        }
    }
}