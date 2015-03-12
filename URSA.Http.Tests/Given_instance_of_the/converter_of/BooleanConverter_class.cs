using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class BooleanConverter_class : ConverterTest<BooleanConverter>
    {
        [TestMethod]
        public void it_should_deserialize_message_as_an_bool()
        {
            bool body = false;
            var result = ConvertTo<bool>("POST", "PostString", "text/plain", body.ToString());

            result.Should().Be(body);
        }

        [TestMethod]
        public void it_should_deserialize_message_as_an_array_of_bools()
        {
            bool[] body = new[] { true, false };
            var result = ConvertTo<bool[]>("POST", "PostStrings", "text/uri-list", String.Join("\r\n", body));

            result.Should().NotBeNull();
            result.Should().HaveCount(body.Length);
            result.First().Should().Be(body.First());
            result.Last().Should().Be(body.Last());
        }

        [TestMethod]
        public void it_should_serialize_a_bool_to_message()
        {
            bool body = true;
            var content = ConvertFrom<bool>("POST", "PostStrings", "text/plain", body);

            content.Should().Be(body.ToString());
        }

        [TestMethod]
        public void it_should_serialize_array_of_bools_to_message()
        {
            bool[] body = new bool[] { false, true };
            var content = ConvertFrom<bool[]>("POST", "PostStrings", "text/plain", body);

            content.Should().Be(String.Join("\r\n", body));
        }
    }
}