using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Security;
using URSA.Web;
using URSA.Web.Converters;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Mapping;
using URSA.Web.Http.Security;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RequestHandler_class
    {
        private Mock<IDelegateMapper<RequestInfo>> _delegateMapper;
        private Mock<IArgumentBinder<RequestInfo>> _argumentBinder;
        private object[] _arguments;
        private Mock<IRequestMapping> _mapping;
        private Mock<IResponseComposer> _responseComposer;

        [TestMethod]
        public void it_should_use_a_response_composer_to_return_operation_result()
        {
            var handler = SetupEnvironment(1, true);

            handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream(), new BasicClaimBasedIdentity()));

            _responseComposer.Verify(instance => instance.ComposeResponse(It.IsAny<IRequestMapping>(), It.IsAny<object>(), It.IsAny<object[]>()), Times.Once);
        }

        [TestMethod]
        public void it_should_use_argument_binder_to_prepare_input_parameters()
        {
            var handler = SetupEnvironment(1, true);

            handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream(), new BasicClaimBasedIdentity()));

            _argumentBinder.Verify(instance => instance.BindArguments(It.IsAny<RequestInfo>(), It.IsAny<IRequestMapping>()), Times.Once);
        }

        [TestMethod]
        public void it_should_use_delegate_mapper_to_call_the_controller_method()
        {
            var handler = SetupEnvironment(1, true);

            handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream(), new BasicClaimBasedIdentity()));

            _delegateMapper.Verify(instance => instance.MapRequest(It.IsAny<RequestInfo>()), Times.Once);
        }

        [TestMethod]
        public void it_should_unwrap_async_tasks_result()
        {
            var expected = 1;
            var handler = SetupEnvironmentAsync(expected, true);

            var result = handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream(), new BasicClaimBasedIdentity()));

            result.Should().BeOfType<ObjectResponseInfo<int>>();
            ((ObjectResponseInfo<int>)result).Value.Should().Be(expected);
        }

        [TestMethod]
        public void it_should_deny_access_to_secured_resourcefor_an_unauthenticated_identity()
        {
            var handler = SetupEnvironment((object)null, true, "Secured");

            var result = handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream(), new BasicClaimBasedIdentity()));

            result.Should().BeOfType<ExceptionResponseInfo>();
            ((ExceptionResponseInfo)result).Value.Should().BeOfType<AccessDeniedException>();
        }

        [TestMethod]
        public void it_should_deny_access_to_secured_resourcefor_an_authenticated_identity()
        {
            var handler = SetupEnvironment((object)null, true, "Secured");

            var result = handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream(), new BasicClaimBasedIdentity("test")));

            result.Should().BeOfType<ExceptionResponseInfo>();
            ((ExceptionResponseInfo)result).Value.Should().BeOfType<AccessDeniedException>();
        }

        [TestMethod]
        public void it_should_deny_access_for_unauthenticated_identity()
        {
            var defaultAuthenticationScheme = new Mock<IDefaultAuthenticationScheme>(MockBehavior.Strict);
            defaultAuthenticationScheme.Setup(instance => instance.Process(It.IsAny<IResponseInfo>())).Returns(Task.FromResult(0));
            var handler = SetupEnvironment((object)null, true, "Authenticated", postRequestHandler: defaultAuthenticationScheme.Object);

            var result = handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream(), new BasicClaimBasedIdentity()));

            result.Should().BeOfType<ExceptionResponseInfo>();
            ((ExceptionResponseInfo)result).Value.Should().BeOfType<UnauthenticatedAccessException>();
            defaultAuthenticationScheme.Verify(instance => instance.Process(It.IsAny<IResponseInfo>()), Times.Once);
        }

        [TestMethod]
        public void it_should_allow_authenticated_identity_to_access_a_resource()
        {
            var handler = SetupEnvironment((object)null, true, "Authenticated");

            var result = handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream(), new BasicClaimBasedIdentity("test")));

            result.Should().NotBeOfType<ExceptionResponseInfo>();
        }

        [TestMethod]
        public void it_should_process_pre_request_handlers()
        {
            var preRequestHandler = new Mock<IPreRequestHandler>(MockBehavior.Strict);
            preRequestHandler.Setup(instance => instance.Process(It.IsAny<IRequestInfo>())).Returns(Task.FromResult(0));
            var handler = SetupEnvironment(1, true, preRequestHandler: preRequestHandler.Object);

            handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream(), new BasicClaimBasedIdentity()));

            preRequestHandler.Verify(instance => instance.Process(It.IsAny<IRequestInfo>()), Times.Once);
        }

        [TestMethod]
        public void it_should_process_post_request_handlers()
        {
            var postRequestHandler = new Mock<IPostRequestHandler>(MockBehavior.Strict);
            postRequestHandler.Setup(instance => instance.Process(It.IsAny<IResponseInfo>())).Returns(Task.FromResult(0));
            var handler = SetupEnvironment(1, true, postRequestHandler: postRequestHandler.Object);

            handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream(), new BasicClaimBasedIdentity()));

            postRequestHandler.Verify(instance => instance.Process(It.IsAny<IResponseInfo>()), Times.Once);
        }

        private RequestHandler SetupEnvironmentAsync<T>(T result = default(T), bool useDefaultArguments = false)
        {
            var converter = new Mock<IConverter>(MockBehavior.Strict);
            converter.Setup(instance => instance.ConvertFrom(result, It.IsAny<IResponseInfo>()));
            var converterProvider = new Mock<IConverterProvider>(MockBehavior.Strict);
            converterProvider.Setup(instance => instance.FindBestOutputConverter<T>(It.IsAny<IResponseInfo>())).Returns(converter.Object);
            var requestHandler = SetupEnvironment(Task.FromResult(result), useDefaultArguments);
            _mapping.Setup(instance => instance.Invoke(_arguments)).Returns(result);
            _responseComposer.Setup(instance => instance.ComposeResponse(_mapping.Object, result, _arguments))
                .Returns(() => ObjectResponseInfo<int>.CreateInstance(Encoding.UTF8, (RequestInfo)_mapping.Object.Target.Response.Request, result, converterProvider.Object));
            return requestHandler;
        }

        private RequestHandler SetupEnvironment<T>(
            T result = default(T),
            bool useDefaultArguments = false,
            string methodName = "Substract",
            IPreRequestHandler preRequestHandler = null,
            IPostRequestHandler postRequestHandler = null)
        {
            var operation = CreateOperation(methodName);
            _arguments = operation.UnderlyingMethod.GetParameters().Select(parameter =>
                (useDefaultArguments ? Activator.CreateInstance(parameter.ParameterType) : null)).ToArray(); 

            ResponseInfo response = null;
            Mock<IController> controller = new Mock<IController>(MockBehavior.Strict);
            controller.SetupGet(instance => instance.Response).Returns(() => response);
            controller.SetupSet(instance => instance.Response = It.IsAny<ResponseInfo>()).Callback<IResponseInfo>(info => response = (ResponseInfo)info);

            _mapping = new Mock<IRequestMapping>(MockBehavior.Strict);
            _mapping.SetupGet(instance => instance.Operation).Returns(operation);
            _mapping.SetupGet(instance => instance.Target).Returns(controller.Object);
            _mapping.Setup(instance => instance.Invoke(_arguments)).Returns(result);

            _delegateMapper = new Mock<IDelegateMapper<RequestInfo>>(MockBehavior.Strict);
            _delegateMapper.Setup(instance => instance.MapRequest(It.IsAny<RequestInfo>())).Returns<RequestInfo>(request => _mapping.Object);

            _argumentBinder = new Mock<IArgumentBinder<RequestInfo>>();
            _argumentBinder.Setup(instance => instance.BindArguments(It.IsAny<RequestInfo>(), It.IsAny<IRequestMapping>()))
                .Returns<IRequestInfo, IRequestMapping>((request, requestMapping) => _arguments);

            _responseComposer = new Mock<IResponseComposer>(MockBehavior.Strict);
            _responseComposer.Setup(instance => instance.ComposeResponse(_mapping.Object, result, _arguments)).Returns((ResponseInfo)null);

            return new RequestHandler(
                _argumentBinder.Object,
                _delegateMapper.Object,
                _responseComposer.Object,
                (preRequestHandler != null ? new[] { preRequestHandler } : null),
                (postRequestHandler != null ? new[] { postRequestHandler } : null));
        }

        private OperationInfo<Verb> CreateOperation(string methodName)
        {
            var method = typeof(TestController).GetMethod(methodName);
            var arguments = method.GetParameters().Select(parameter => (ValueInfo)new ArgumentInfo(parameter, FromQueryStringAttribute.For(parameter), "test", "test"));
            return new OperationInfo<Verb>(
                method,
                new Uri("/", UriKind.Relative),
                (arguments.Any() ? "test" : null),
                new Regex(".*"),
                Verb.GET,
                arguments.ToArray())
                .WithSecurityDetailsFrom(method);
        }
    }
}