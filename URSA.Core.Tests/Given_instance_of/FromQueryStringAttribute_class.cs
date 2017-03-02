using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using URSA.Tests.Web;
using URSA.Web.Mapping;

namespace Given_instance_of
{
    [TestFixture]
    public class FromQueryStringAttribute_class
    {
        private static readonly ParameterInfo Parameter = typeof(TestController).GetMethod("Result").GetParameters().First();

        [Test]
        public void it_should_throw_when_no_url_is_provided()
        {
            ((FromQueryStringAttribute)null).Invoking(_ => new FromQueryStringAttribute(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("url");
        }

        [Test]
        public void it_should_throw_when_url_provided_is_empty()
        {
            ((FromQueryStringAttribute)null).Invoking(_ => new FromQueryStringAttribute(String.Empty)).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("url");
        }

        [Test]
        public void it_should_throw_when_url_provided_is_has_no_variable()
        {
            ((FromQueryStringAttribute)null).Invoking(_ => new FromQueryStringAttribute("/")).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("url");
        }

        [Test]
        public void it_should_throw_when_no_parameter_is_provided()
        {
            ((FromQueryStringAttribute)null).Invoking(_ => FromQueryStringAttribute.For(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("parameter");
        }

        [Test]
        public void it_should_create_an_instance_correctly()
        {
            var result = FromQueryStringAttribute.For(Parameter);

            result.Should().BeOfType<FromQueryStringAttribute>().Which.UrlTemplate.Should().Be("{?input}");
        }
    }
}