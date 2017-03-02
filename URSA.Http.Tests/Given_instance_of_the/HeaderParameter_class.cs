using System;
using FluentAssertions;
using NUnit.Framework;
using URSA.Web.Http;

namespace Given_instance_of_the
{
    [TestFixture]
    public class HeaderParameter_class
    {
        [Test]
        public void it_should_try_and_parse_a_header_value()
        {
            HeaderParameter result;
            HeaderParameter.TryParse("q=1", out result).Should().BeTrue();

            result.Should().BeOfType<HeaderParameter>().Which.Name.Should().Be("q");
            result.Value.Should().Be(1.0);
        }

        [Test]
        public void it_should_throw_when_no_parameter_is_passed_for_parsing()
        {
            ((HeaderParameter)null).Invoking(_ => HeaderParameter.Parse(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("parameter");
        }

        [Test]
        public void it_should_throw_when_parameter_passed_for_parsing_is_empty()
        {
            ((HeaderParameter)null).Invoking(_ => HeaderParameter.Parse(String.Empty)).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("parameter");
        }

        [Test]
        public void it_should_throw_when_parameter_has_no_name()
        {
            ((HeaderParameter)null).Invoking(_ => HeaderParameter.Parse("=")).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("parameter");
        }

        [Test]
        public void it_should_throw_when_value_of_the_parameter_is_empty()
        {
            ((HeaderParameter)null).Invoking(_ => HeaderParameter.Parse("param=")).ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void it_should_consider_two_parametes_equal()
        {
            new HeaderParameter("key", 1).Equals(new HeaderParameter("key", 1)).Should().BeTrue();
        }

        [Test]
        public void it_should_consider_same_instance_as_equal()
        {
            var param = new HeaderParameter("key", 1);
            param.Equals(param).Should().BeTrue();
        }

        [Test]
        public void it_should_not_consider_as_equal_two_different_objects()
        {
            new HeaderParameter("key", 1).Equals("key").Should().BeFalse();
        }

        [Test]
        public void it_should_parse_Uri_correctly()
        {
            var expected = "http://temp.uri/";
            HeaderParameter.Parse("key=<" + expected + ">").Value.Should().BeOfType<Uri>().Which.AbsoluteUri.Should().Be(expected);
        }
    }
}