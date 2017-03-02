using FluentAssertions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;
using URSA.Web.Http;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class Header_class
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void it_should_parse_the_header_correctly()
        {
            var header = Header.Parse("Accept: text/plain; q=0.5, text/html,\r\n text/x-dvi; q=\"test\", text/x-c,text/csv");

            header.Should().NotBeNull();
            header.Name.Should().Be("Accept");
            header.Values.Should().HaveCount(5);

            header.Values.First().Value.Should().Be("text/plain");
            header.Values.First().Parameters.Should().HaveCount(1);
            header.Values.First().Parameters.First().Name.Should().Be("q");
            header.Values.First().Parameters["q"].Value.Should().Be(0.5);

            header.Values.Skip(1).First().Value.Should().Be("text/html");

            header.Values.Skip(2).First().Value.Should().Be("text/x-dvi");
            header.Values.Skip(2).First().Parameters.Should().HaveCount(1);
            header.Values.Skip(2).First().Parameters.First().Name.Should().Be("q");
            header.Values.Skip(2).First().Parameters["q"].Value.Should().Be("test");

            header.Values.Skip(3).First().Value.Should().Be("text/x-c");

            header.Values.Last().Value.Should().Be("text/csv");
        }

        [Test]
        public void it_should_try_and_parse_the_header_with_text_parameter_correctly()
        {
            Header header;
            Header.TryParse("Pragma: no-cache; reason=\"This is some\\n custom \\\" \\, text.\"", out header).Should().BeTrue();

            header.Should().NotBeNull();
            header.Name.Should().Be("Pragma");
            header.Values.Should().HaveCount(1);

            header.Values.First().Value.Should().Be("no-cache");
            header.Values.First().Parameters.Should().HaveCount(1);
            header.Values.First().Parameters.First().Name.Should().Be("reason");
            header.Values.First().Parameters["reason"].Value.Should().Be("This is some\n custom \\\" \\, text.");
        }

        [Test]
        public void it_should_throw_when_no_header_to_be_parsed_is_provided()
        {
            ((Header)null).Invoking(_ => Header.Parse(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("header");
        }

        [Test]
        public void it_should_throw_when_header_to_be_parsed_provided_is_empty()
        {
            ((Header)null).Invoking(_ => Header.Parse(String.Empty)).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("header");
        }

        [Test]
        public void it_should_throw_when_header_to_be_parsed_provided_has_no_name()
        {
            ((Header)null).Invoking(_ => Header.Parse("test")).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("header");
        }

        [Test]
        public void it_should_throw_when_invalid_Content_type_header_is_parsed()
        {
            ((Header)null).Invoking(_ => new Header("Content-Length")).ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void it_should_consider_two_same_headers_as_equal()
        {
            new Header("test").Equals(new Header("test")).Should().BeTrue();
        }

        [Test]
        public void it_should_consider_same_instance_as_equal()
        {
            var instance = new Header("test");
            instance.Equals(instance).Should().BeTrue();
        }

        [Test]
        public void it_should_not_consider_two_objects_of_different_type_as_equal()
        {
            new Header("test").Equals("test").Should().BeFalse();
        }

        [Test]
        public void it_should_maintain_parsed_header_values_correctly()
        {
            var header = new Header<int>("Content-Length", 0);

            header.Values.Add(new HeaderValue<int>(10));
            header.Values.Remove(header.Values.First(item => item.Value == 0));

            header.Values.Should().HaveCount(1).And.Subject.First().Value.Should().Be(10);
        }

        [Test]
        public void it_should_maintain_serialized_header_values_correctly()
        {
            var header = (Header)new Header<int>("Content-Length", 0);

            header.Values.Add(new HeaderValue<int>(10));
            header.Values.Remove(header.Values.First(item => item.Value == "0"));

            ((Header<int>)header).Values.Should().HaveCount(1).And.Subject.First().Value.Should().Be(10);
        }

        [Test]
        public void it_should_throw_when_last_parsed_value_is_removed()
        {
            var header = new Header<int>("Content-Length", 0);

            header.Values.Invoking(values => values.Remove(header.Values.First(item => item.Value == 0))).ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void it_should_throw_when_last_value_is_removed()
        {
            var header = (Header)new Header<int>("Content-Length", 0);

            header.Values.Invoking(values => values.Remove(header.Values.First(item => item.Value == "0"))).ShouldThrow<InvalidOperationException>();
        }
    }
}