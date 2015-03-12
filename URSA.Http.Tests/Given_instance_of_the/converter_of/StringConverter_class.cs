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
    public class StringConverter_class : ConverterTest<StringConverter>
    {
        [TestMethod]
        public void it_should_deserialize_message_as_a_string()
        {
            string body = "test string";
            var result = ConvertTo<string>("POST", "PostString", "text/plain", body);

            result.Should().NotBeNull();
            result.Should().Be(body);
        }

        [TestMethod]
        public void it_should_deserialize_message_as_an_array_of_strings()
        {
            string[] body = new[] { "test string 1", "test string 2" };
            var result = ConvertTo<string[]>("POST", "PostStrings", "text/plain", String.Join("\r\n", body));

            result.Should().NotBeNull();
            result.Should().HaveCount(body.Length);
            result.First().Should().Be(body.First());
            result.Last().Should().Be(body.Last());
        }

        [TestMethod]
        public void it_should_serialize_a_string_to_message()
        {
            string body = "test";
            var content = ConvertFrom<string>("POST", "PostStrings", "application/xml", body);

            content.Should().Be(body);
        }

        [TestMethod]
        public void it_should_serialize_array_of_strings_to_message()
        {
            string[] body = new[] { "test1", "test2" };
            var content = ConvertFrom<string[]>("POST", "PostStrings", "text/plain", body);

            content.Should().Be(String.Join("\r\n", body));
        }
    }
}