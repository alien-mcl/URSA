using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using URSA.Security;
using URSA.Web;
using URSA.Web.Http;
using URSA.Web.Tests;

namespace Given_instance_of_the.ResponseComposer_class
{
    [TestClass]
    public class when_dealing_with_normal_controller : ResponseComposerTest<TestController>
    {
        [TestMethod]
        public void it_should_throw_when_no_request_mapping_is_provided()
        {
            Composer.Invoking(instance => instance.ComposeResponse(null, null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("requestMapping");
        }

        [TestMethod]
        public void it_should_merge_headers_if_the_output_is_already_a_response()
        {
            var expected = "http://temp.uri/";
            var output = new StringResponseInfo(String.Empty, new RequestInfo(Verb.GET, new Uri("/", UriKind.Relative), new MemoryStream(), new BasicClaimBasedIdentity()));
            var controller = new Mock<IController>(MockBehavior.Strict);
            controller.SetupGet(instance => instance.Response).Returns(output);
            var mapping = new Mock<IRequestMapping>(MockBehavior.Strict);
            mapping.SetupGet(instance => instance.Target).Returns(controller.Object);
            output.Headers.Location = expected;

            var result = Composer.ComposeResponse(mapping.Object, output);

            result.Headers["Location"].Value.Should().Be(expected);
        }

        [TestMethod]
        public void it_should_serialize_result_to_body()
        {
            object[] arguments = { 1, 2 };
            int expected = 3;
            Converter.Setup(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()));

            var result = Composer.ComposeResponse(CreateRequestMapping("Add", arguments), expected, arguments);

            result.Should().BeOfType<ObjectResponseInfo<int>>();
            Converter.Verify(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()), Times.Once);
        }

        [TestMethod]
        public void it_should_serialize_result_to_header()
        {
            object[] arguments = { 1, 2 };
            int expected = 3;

            var result = Composer.ComposeResponse(CreateRequestMapping("Multiply", arguments), expected, arguments);

            result.Should().BeOfType<StringResponseInfo>();
            Converter.Verify(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()), Times.Never);
            result.Headers.Should().ContainKey("Pragma").WhichValue.Should().Be(expected.ToString());
        }

        [TestMethod]
        public void it_should_serialize_result_to_both_header_and_body()
        {
            int expected = 3;
            var arguments = new object[] { 1, 2 };
            Converter.Setup(instance => instance.ConvertFrom(It.IsAny<int>(), It.IsAny<IResponseInfo>()));

            var result = Composer.ComposeResponse(CreateRequestMapping("Whatever", arguments), expected, arguments);

            result.Should().BeOfType<MultiObjectResponseInfo>();
            Converter.Verify(instance => instance.ConvertFrom(It.IsAny<int>(), It.IsAny<IResponseInfo>()), Times.Exactly(arguments.Length));
            result.Headers.Should().ContainKey("Location").WhichValue.Should().Be(arguments.Last().ToString());
            var multiObjectResult = (MultiObjectResponseInfo)result;
            multiObjectResult.Values.Should().HaveCount(2);
            multiObjectResult.Values.First().Should().Be(arguments.First());
            multiObjectResult.Values.Last().Should().Be(expected);
        }
    }
}