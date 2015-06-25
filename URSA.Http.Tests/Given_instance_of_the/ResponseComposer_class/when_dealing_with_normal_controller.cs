using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using URSA.Web;
using URSA.Web.Http;
using URSA.Web.Tests;

namespace Given_instance_of_the.ResponseComposer_class
{
    [TestClass]
    public class when_dealing_with_normal_controller : ResponseComposerTest<TestController>
    {
        [TestMethod]
        public void it_should_serialize_result_to_body()
        {
            object[] arguments = { 1, 2 };
            int expected = 3;
            Converter.Setup(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()));

            var result = Composer.ComposeResponse(CreateRequestMapping("Add", expected, arguments), expected, arguments);

            result.Should().BeOfType<ObjectResponseInfo<int>>();
            Converter.Verify(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()), Times.Once);
        }

        [TestMethod]
        public void it_should_serialize_result_to_header()
        {
            object[] arguments = { 1, 2 };
            int expected = 3;

            var result = Composer.ComposeResponse(CreateRequestMapping("Multiply", expected, arguments), expected, arguments);

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

            var result = Composer.ComposeResponse(CreateRequestMapping("Whatever", expected, arguments), expected, arguments);

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