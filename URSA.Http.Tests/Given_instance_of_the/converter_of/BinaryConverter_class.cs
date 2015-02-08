using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [TestClass]
    public class BinaryConverter_class : ConverterTest<BinaryConverter>
    {
        [TestMethod]
        public void it_should_deserialize_message_as_an_array_of_bytes()
        {
            byte[] body = new byte[] { 1, 2 };
            var result = ConvertTo<byte[]>("POST", "PostString", "text/plain", System.Convert.ToBase64String(body));

            result.Should().NotBeNull();
            result.Should().HaveCount(body.Length);
            result.First().Should().Be(body.First());
            result.Last().Should().Be(body.Last());
        }

        [TestMethod]
        public void it_should_serialize_array_of_bytes_to_message()
        {
            byte[] body = new byte[] { 1, 2 };
            var content = ConvertFrom<byte[]>("POST", "PostStrings", "text/plain", body);

            content.Should().Be(System.Convert.ToBase64String(body));
        }
    }
}