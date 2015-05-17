using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using URSA.Web;
using URSA.Web.Converters;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Tests.Testing;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the
{
    [TestClass]
    public class ResponseComposer_class
    {
        private Mock<IConverter> _converter;
        private Mock<IConverterProvider> _converterProvider;
        private Mock<IHttpControllerDescriptionBuilder<TestController>> _controllerDescriptionBuilder;
        private TestController _controller;
        private ResponseComposer _composer;

        [TestMethod]
        public void it_should_serialize_result_to_body()
        {
            object[] arguments = { 1, 2 };
            int expected = 3;
            _converter.Setup(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()));

            var result = _composer.ComposeResponse(CreateRequestMapping("Add", expected, arguments), expected, arguments);

            result.Should().BeOfType<ObjectResponseInfo<int>>();
            _converter.Verify(instance => instance.ConvertFrom(3, It.IsAny<IResponseInfo>()), Times.Once);
        }

        [TestMethod]
        public void it_should_serialize_result_to_header()
        {
            object[] arguments = { 1, 2 };
            int expected = 3;

            var result = _composer.ComposeResponse(CreateRequestMapping("Multiply", expected, arguments), expected, arguments);

            result.Should().BeOfType<StringResponseInfo>();
            _converter.Verify(instance => instance.ConvertFrom(3, It.IsAny<IResponseInfo>()), Times.Never);
            result.Headers.Should().ContainKey("Pragma").WhichValue.Should().Be(expected.ToString());
        }

        [TestMethod]
        public void it_should_serialize_result_to_both_header_and_body()
        {
            int expected = 3;
            var arguments = new object[] { 1, 2 };
            _converter.Setup(instance => instance.ConvertFrom(It.IsAny<int>(), It.IsAny<IResponseInfo>()));

            var result = _composer.ComposeResponse(CreateRequestMapping("Whatever", expected, arguments), expected, arguments);

            result.Should().BeOfType<MultiObjectResponseInfo>();
            _converter.Verify(instance => instance.ConvertFrom(It.IsAny<int>(), It.IsAny<IResponseInfo>()), Times.Exactly(arguments.Length));
            result.Headers.Should().ContainKey("Location").WhichValue.Should().Be(arguments.Last().ToString());
            var multiObjectResult = (MultiObjectResponseInfo)result;
            multiObjectResult.Values.Should().HaveCount(2);
            multiObjectResult.Values.First().Should().Be(arguments.First());
            multiObjectResult.Values.Last().Should().Be(expected);
        }

        //// TODO: Implement unit tests for CRUD operations.
        [TestMethod]
        public void it_should_handle_List_request_correctly()
        {
        }

        [TestMethod]
        public void it_should_handle_Read_request_correctly()
        {
        }

        [TestMethod]
        public void it_should_handle_Create_request_correctly()
        {
        }

        [TestMethod]
        public void it_should_handle_Update_request_correctly()
        {
        }

        [TestMethod]
        public void it_should_handle_Delete_request_correctly()
        {
        }

        [TestInitialize]
        public void Setup()
        {
            _converter = new Mock<IConverter>(MockBehavior.Strict);
            _converterProvider = new Mock<IConverterProvider>(MockBehavior.Strict);
            _converterProvider.Setup(instance => instance.FindBestOutputConverter<int>(It.IsAny<IResponseInfo>())).Returns(_converter.Object);
            _converterProvider.Setup(instance => instance.FindBestOutputConverter<double>(It.IsAny<IResponseInfo>())).Returns(_converter.Object);
            _converterProvider.Setup(instance => instance.FindBestOutputConverter<string>(It.IsAny<IResponseInfo>())).Returns(_converter.Object);
            _converterProvider.Setup(instance => instance.FindBestOutputConverter<Guid>(It.IsAny<IResponseInfo>())).Returns(_converter.Object);
            _converterProvider.Setup(instance => instance.FindBestOutputConverter<object>(It.IsAny<IResponseInfo>())).Returns(_converter.Object);
            _controllerDescriptionBuilder = new Mock<IHttpControllerDescriptionBuilder<TestController>>(MockBehavior.Strict);
            _controller = new TestController { Response = new StringResponseInfo(Encoding.UTF8, null, new RequestInfo(Verb.GET, new Uri("http://temp.uri/api"), new MemoryStream())) };
            _composer = new ResponseComposer(_converterProvider.Object, new[] { _controllerDescriptionBuilder.Object });
        }

        [TestCleanup]
        public void Teardown()
        {
            _composer = null;
            _converterProvider = null;
        }

        private RequestMapping CreateRequestMapping(string methodName, object expectedResult, params object[] arguments)
        {
            var method = _controller.GetType().GetMethod(methodName);
            string baseUri = _controller.GetType().GetCustomAttribute<RouteAttribute>().Uri.ToString();
            string callUri;
            return new RequestMapping(_controller, method.ToOperationInfo(baseUri, Verb.GET, out callUri, arguments), new Uri(callUri.TrimStart('/'), UriKind.Relative));
        }
    }
}