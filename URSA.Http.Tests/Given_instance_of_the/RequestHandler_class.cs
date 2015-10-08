using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web;
using URSA.Web.Converters;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Mapping;
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
        private Mock<IResponseComposer> _responseComposer;

        [TestMethod]
        public void it_should_use_a_response_composer_to_return_operation_result()
        {
            var handler = SetupEnvironment(true);

            handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream()));

            _responseComposer.Verify(instance => instance.ComposeResponse(It.IsAny<IRequestMapping>(), It.IsAny<object>(), It.IsAny<object[]>()), Times.Once);
        }

        [TestMethod]
        public void it_should_use_argument_binder_to_prepare_input_parameters()
        {
            var handler = SetupEnvironment(true);

            handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream()));

            _argumentBinder.Verify(instance => instance.BindArguments(It.IsAny<RequestInfo>(), It.IsAny<IRequestMapping>()), Times.Once);
        }

        [TestMethod]
        public void it_should_use_delegate_mapper_to_call_the_controller_method()
        {
            var handler = SetupEnvironment(true);

            handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream()));

            _delegateMapper.Verify(instance => instance.MapRequest(It.IsAny<RequestInfo>()), Times.Once);
        }

        private RequestHandler SetupEnvironment(bool useDefaultArguments = false)
        {
            var operation = CreateOperation();
            var arguments = operation.UnderlyingMethod.GetParameters().Select(parameter =>
                (useDefaultArguments ? Activator.CreateInstance(parameter.ParameterType) : null)).ToArray(); 

            ResponseInfo response = null;
            Mock<IController> controller = new Mock<IController>(MockBehavior.Strict);
            controller.SetupGet(instance => instance.Response).Returns(() => response);
            controller.SetupSet(instance => instance.Response = It.IsAny<ResponseInfo>()).Callback<IResponseInfo>(info => response = (ResponseInfo)info);

            int result = 1;
            Mock<IRequestMapping> mapping = new Mock<IRequestMapping>(MockBehavior.Strict);
            mapping.SetupGet(instance => instance.Operation).Returns(operation);
            mapping.SetupGet(instance => instance.Target).Returns(controller.Object);
            mapping.Setup(instance => instance.Invoke(arguments)).Returns(result);

            _delegateMapper = new Mock<IDelegateMapper<RequestInfo>>(MockBehavior.Strict);
            _delegateMapper.Setup(instance => instance.MapRequest(It.IsAny<RequestInfo>())).Returns<RequestInfo>(request => mapping.Object);

            _argumentBinder = new Mock<IArgumentBinder<RequestInfo>>();
            _argumentBinder.Setup(instance => instance.BindArguments(It.IsAny<RequestInfo>(), It.IsAny<IRequestMapping>()))
                .Returns<IRequestInfo, IRequestMapping>((request, requestMapping) => arguments);

            Mock<IConverter> converter = new Mock<IConverter>(MockBehavior.Strict);
            converter.Setup(instance => instance.CanConvertFrom(typeof(int), It.IsAny<ResponseInfo>())).Returns(CompatibilityLevel.ExactMatch);
            converter.Setup(instance => instance.ConvertFrom<int>(result, It.IsAny<ResponseInfo>()));

            Mock<IConverterProvider> converterProvider = new Mock<IConverterProvider>(MockBehavior.Strict);
            converterProvider.Setup(instance => instance.FindBestOutputConverter<int>(It.IsAny<IResponseInfo>())).Returns(converter.Object);

            Mock<IHttpControllerDescriptionBuilder<TestController>> builder = new Mock<IHttpControllerDescriptionBuilder<TestController>>(MockBehavior.Strict);
            Mock<IControllerActivator> activator = new Mock<IControllerActivator>(MockBehavior.Strict);

            Mock<IComponentProvider> container = new Mock<IComponentProvider>(MockBehavior.Strict);
            container.Setup(instance => instance.Resolve<IConverterProvider>(null)).Returns(converterProvider.Object);
            container.Setup(instance => instance.Resolve<IDelegateMapper<RequestInfo>>(null)).Returns(_delegateMapper.Object);
            container.Setup(instance => instance.Register<IDelegateMapper<RequestInfo>>(It.IsAny<DelegateMapper>()));
            container.Setup(instance => instance.Resolve<IArgumentBinder<RequestInfo>>(null)).Returns(_argumentBinder.Object);
            container.Setup(instance => instance.Register<IArgumentBinder<RequestInfo>>(It.IsAny<ArgumentBinder>()));
            container.Setup(instance => instance.Register<IConverterProvider>(It.IsAny<IConverterProvider>()));
            container.Setup(instance => instance.Register<IControllerActivator>(It.IsAny<IControllerActivator>()));
            container.Setup(instance => instance.Resolve<IControllerActivator>(null)).Returns(activator.Object);
            container.Setup(instance => instance.ResolveAll<IParameterSourceArgumentBinder>(null)).Returns(new IParameterSourceArgumentBinder[0]);
            container.Setup(instance => instance.ResolveAll<IConverter>(null)).Returns(new IConverter[] { converter.Object });
            container.Setup(instance => instance.ResolveAllTypes<IController>()).Returns(new Type[] { typeof(TestController) });
            container.Setup(instance => instance.Resolve(typeof(IHttpControllerDescriptionBuilder<>).MakeGenericType(typeof(TestController)), null)).Returns(builder.Object);
            UrsaConfigurationSection.ComponentProvider = container.Object;

            _responseComposer = new Mock<IResponseComposer>(MockBehavior.Strict);
            _responseComposer.Setup(instance => instance.ComposeResponse(mapping.Object, result, arguments)).Returns((ResponseInfo)null);
            return new RequestHandler(_argumentBinder.Object, _delegateMapper.Object, _responseComposer.Object);
        }

        private OperationInfo<Verb> CreateOperation()
        {
            var method = typeof(TestController).GetMethod("Substract");
            var arguments = method.GetParameters().Select(parameter => new ArgumentInfo(parameter, FromQueryStringAttribute.For(parameter), "test", "test")).ToArray();
            return new OperationInfo<Verb>(method, new Uri("/", UriKind.Relative), "test", new Regex(".*"), Verb.GET, arguments);
        }
    }
}