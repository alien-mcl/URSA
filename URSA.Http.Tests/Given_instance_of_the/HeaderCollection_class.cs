using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using URSA.Web.Http;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class HeaderCollection_class
    {
        [TestMethod]
        public void it_should_set_a_header()
        {
            HeaderCollection headers = new HeaderCollection();
            headers.Set(new Header("Header1", new[] { new HeaderValue("Value1", new HeaderParameter("Parameter1", "ParameterValue1")) }));
            headers.Set(new Header("Header1", "Value2"));

            headers.Count.Should().Be(1);
            headers["Header1"].Values.Count.Should().Be(1);
            headers["Header1"].Values.First().Parameters.Count.Should().Be(0);
        }

        [TestMethod]
        public void it_should_add_a_header()
        {
            HeaderCollection headers = new HeaderCollection();
            headers.Add(new Header("Header1", new List<HeaderValue>(new[] { new HeaderValue("Value1", new HeaderParameter("Parameter1", "ParameterValue1")) })));
            headers.Add("Header1", "Value2");

            headers.Count.Should().Be(1);
            headers["Header1"].Values.Count.Should().Be(2);
            headers["Header1"].Values.First().Parameters.Count.Should().Be(1);
        }

        [TestMethod]
        public void it_should_remove_a_header()
        {
            HeaderCollection headers = new HeaderCollection();
            headers.Add(new Header("Header1", new[] { new HeaderValue("Value1", new HeaderParameter("Parameter1", "ParameterValue1")) }));
            headers.Remove("Header1");

            headers.Count.Should().Be(0);
        }

        [TestMethod]
        public void it_should_get_a_header_by_its_name()
        {
            HeaderCollection headers = new HeaderCollection();
            headers.Add(new Header("Header1", new[] { new HeaderValue("Value1", new HeaderParameter("Parameter1", "ParameterValue1")) }));

            Header header = headers["Header1"];

            header.Should().NotBeNull();
            header.Name.Should().Be("Header1");
        }

        [TestMethod]
        public void it_should_serialize_the_headers_to_its_string_representation()
        {
            HeaderCollection headers = new HeaderCollection();
            headers.Add(new Header("Header1", new[] { new HeaderValue("Value1", new HeaderParameter("Parameter1", "ParameterValue1")) }));
            headers.Add(new Header("Header2", new[] { new HeaderValue("Value2", new HeaderParameter("Parameter2", "ParameterValue2")) }));

            string expected = String.Format("{0}\r\n{1}\r\n\r\n", headers["Header1"], headers["Header2"]);
            headers.ToString().Should().Be(expected);
        }

        [TestMethod]
        public void it_should_try_and_parse_headers_correctly()
        {
            HeaderCollection headers;
            HeaderCollection.TryParse(
                "Accept: text/plain; q=0.5, text/html,\r\n text/x-dvi; q=\"test\", text/x-c\r\n" +
                "Pragma: no-cache; reason=\"This is some\\n custom text.\"\r\n" +
                "Accept-Language: pl",
                out headers).Should().BeTrue();

            headers.Should().HaveCount(3);
            var accept = headers["Accept"];
            var pragma = headers["Pragma"];
            var acceptLanguage = headers["Accept-Language"];

            accept.Should().NotBeNull();
            accept.Name.Should().Be("Accept");
            accept.Values.Should().HaveCount(4);

            accept.Values.First().Value.Should().Be("text/plain");
            accept.Values.First().Parameters.Should().HaveCount(1);
            accept.Values.First().Parameters.First().Name.Should().Be("q");
            accept.Values.First().Parameters["q"].Value.Should().Be(0.5);

            accept.Values.Skip(1).First().Value.Should().Be("text/html");

            accept.Values.Skip(2).First().Value.Should().Be("text/x-dvi");
            accept.Values.Skip(2).First().Parameters.Should().HaveCount(1);
            accept.Values.Skip(2).First().Parameters.First().Name.Should().Be("q");
            accept.Values.Skip(2).First().Parameters["q"].Value.Should().Be("test");

            accept.Values.Last().Value.Should().Be("text/x-c");

            pragma.Should().NotBeNull();
            pragma.Name.Should().Be("Pragma");
            pragma.Values.Should().HaveCount(1);

            pragma.Values.First().Value.Should().Be("no-cache");
            pragma.Values.First().Parameters.Should().HaveCount(1);
            pragma.Values.First().Parameters.First().Name.Should().Be("reason");
            pragma.Values.First().Parameters["reason"].Value.Should().Be("This is some\n custom text.");

            acceptLanguage.Should().NotBeNull();
            acceptLanguage.Name.Should().Be("Accept-Language");
            acceptLanguage.Values.Should().HaveCount(1);

            acceptLanguage.Values.First().Value.Should().Be("pl");
        }

        [TestMethod]
        public void it_should_throw_when_no_headers_to_be_parsed_are_passed()
        {
            ((HeaderCollection)null).Invoking(_ => HeaderCollection.Parse(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("headers");
        }

        [TestMethod]
        public void it_should_throw_when_headers_to_be_parsed_are_empty()
        {
            ((HeaderCollection)null).Invoking(_ => HeaderCollection.Parse(String.Empty)).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("headers");
        }

        [TestMethod]
        public void it_should_throw_when_no_header_is_being_added()
        {
            new HeaderCollection().Invoking(instance => instance.Add(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("header");
        }

        [TestMethod]
        public void it_should_throw_when_no_header_is_being_set()
        {
            new HeaderCollection().Invoking(instance => instance.Set(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("header");
        }

        [TestMethod]
        public void it_should_do_nothing_when_no_headers_is_being_merged()
        {
            var headers = new HeaderCollection();

            headers.Merge(null);

            headers.Should().HaveCount(0);
        }

        [TestMethod]
        public void it_should_merge_headers()
        {
            var headers1 = new HeaderCollection(new Header("test1"));
            var headers2 = new HeaderCollection(new Header("test2"));

            headers1.Merge(headers2);

            headers1.Should().ContainKey("test2");
        }

        [TestMethod]
        public void it_should_set_and_get_named_headers_correctly()
        {
            var collection = new HeaderCollection();
            foreach (var namedHeader in collection.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => (property.CanRead) && (property.CanWrite) && (property.Name != "Item")))
            {
                var value = (namedHeader.PropertyType == typeof(string) ? (object)"test" : 1);
                namedHeader.SetValue(collection, value);
                namedHeader.GetValue(collection).Should().Be(value);
            }
        }
    }
}