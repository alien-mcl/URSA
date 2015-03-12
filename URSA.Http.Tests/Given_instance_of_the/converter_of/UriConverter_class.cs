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
    public class UriConverter_class : ConverterTest<UriConverter>
    {
        [TestMethod]
        public void it_should_deserialize_message_as_an_Uri()
        {
            string body = "http://temp.org/";
            var result = ConvertTo<Uri>("POST", "PostString", "text/uri-list", body);

            result.Should().NotBeNull();
            result.Should().Be(body);
        }

        [TestMethod]
        public void it_should_deserialize_message_as_an_array_of_Uris()
        {
            string[] body = new[] { "http://temp.org/", "ftp://temp.org/" };
            var result = ConvertTo<Uri[]>("POST", "PostStrings", "text/uri-list", String.Join("\r\n", body));

            result.Should().NotBeNull();
            result.Should().HaveCount(body.Length);
            result.First().AbsoluteUri.Should().Be(body.First());
            result.Last().AbsoluteUri.Should().Be(body.Last());
        }

        [TestMethod]
        public void it_should_serialize_an_Uri_to_message()
        {
            Uri body = new Uri("http://temp.org/");
            var content = ConvertFrom<Uri>("POST", "PostStrings", "text/uri-list", body);

            content.Should().Be(body.ToString());
        }

        [TestMethod]
        public void it_should_serialize_array_of_Uris_to_message()
        {
            Uri[] body = new Uri[] { new Uri("http://temp.org/"), new Uri("ftp://test.com/test#") };
            var content = ConvertFrom<Uri[]>("POST", "PostStrings", "text/uri-list", body);

            content.Should().Be(String.Join("\r\n", body.Select(item => item.ToString())));
        }
    }
}