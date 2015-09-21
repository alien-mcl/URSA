using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using URSA.Web.Http;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class Header_class
    {
        [TestMethod]
        public void it_should_throw_if_an_empty_header_name_is_provided()
        {
            Exception exception = null;
            try
            {
                new Header(String.Empty);
            }
            catch (Exception error)
            {
                exception = error;
            }

            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void it_should_throw_if_no_header_name_is_provided()
        {
            Exception exception = null;
            try
            {
                new Header(null);
            }
            catch (Exception error)
            {
                exception = error;
            }

            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [TestMethod]
        public void it_should_initialize_the_instance_properly()
        {
            string givenName = "Name";
            string givenParameterName = "ParameterName";
            HeaderValue[] givenValue = new[] { new HeaderValue("Value1"), new HeaderValue("Value2") };
            givenValue[0].Parameters.Add(new HeaderParameter(givenParameterName));
            Header header = new Header(givenName, givenValue);

            header.Name.Should().Be(givenName);
            header.Values.Count.Should().Be(givenValue.Length);
            header.Values.First().Should().Be(givenValue[0]);
            header.Values.First().Parameters.Should().HaveCount(1);
            header.Values.Last().Should().Be(givenValue[1]);
        }

        [TestMethod]
        public void it_should_serialize_the_header_to_its_string_representation()
        {
            string givenName = "Name";
            string givenParameterName = "Parameter";
            string givenParameterValue = "ParameterValue";
            HeaderValue[] givenValue = new[] { new HeaderValue("Value1"), new HeaderValue("Value2") };
            givenValue[0].Parameters.Add(new HeaderParameter(givenParameterName, givenParameterValue));
            Header header = new Header(givenName, givenValue);

            string expected = String.Format("{0}:{1};{3}=\"{4}\",{2}", givenName, givenValue[0].Value, givenValue[1].Value, givenParameterName, givenParameterValue);
            header.ToString().Should().Be(expected);
        }

        [TestMethod]
        public void it_should_parse_the_header_correctly()
        {
            var header = Header.Parse("Accept: text/plain; q=0.5, text/html,\r\n text/x-dvi; q=\"test\", text/x-c");

            header.Should().NotBeNull();
            header.Name.Should().Be("Accept");
            header.Values.Should().HaveCount(4);

            header.Values.First().Value.Should().Be("text/plain");
            header.Values.First().Parameters.Should().HaveCount(1);
            header.Values.First().Parameters.First().Name.Should().Be("q");
            header.Values.First().Parameters["q"].Value.Should().Be(0.5);

            header.Values.Skip(1).First().Value.Should().Be("text/html");

            header.Values.Skip(2).First().Value.Should().Be("text/x-dvi");
            header.Values.Skip(2).First().Parameters.Should().HaveCount(1);
            header.Values.Skip(2).First().Parameters.First().Name.Should().Be("q");
            header.Values.Skip(2).First().Parameters["q"].Value.Should().Be("test");

            header.Values.Last().Value.Should().Be("text/x-c");
        }

        [TestMethod]
        public void it_should_parse_the_header_with_text_parameter_correctly()
        {
            var header = Header.Parse("Pragma: no-cache; reason=\"This is some\\n custom text.\"");

            header.Should().NotBeNull();
            header.Name.Should().Be("Pragma");
            header.Values.Should().HaveCount(1);

            header.Values.First().Value.Should().Be("no-cache");
            header.Values.First().Parameters.Should().HaveCount(1);
            header.Values.First().Parameters.First().Name.Should().Be("reason");
            header.Values.First().Parameters["reason"].Value.Should().Be("This is some\n custom text.");
        }
    }
}