using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Tests.Web;
using URSA.Web.Mapping;

namespace Given_instance_of
{
    [TestClass]
    public class FromUriAttribute_class
    {
        private static readonly ParameterInfo Parameter = typeof(TestController).GetMethod("Result").GetParameters().First();

        [TestMethod]
        public void it_should_throw_when_no_uri_is_provided()
        {
            ((FromUriAttribute)null).Invoking(_ => new FromUriAttribute(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("uri");
        }

        [TestMethod]
        public void it_should_throw_when_uri_provided_is_empty()
        {
            ((FromUriAttribute)null).Invoking(_ => new FromUriAttribute(String.Empty)).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("uri");
        }

        [TestMethod]
        public void it_should_throw_when_uri_provided_is_has_no_variable()
        {
            ((FromUriAttribute)null).Invoking(_ => new FromUriAttribute("/")).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("uri");
        }

        [TestMethod]
        public void it_should_throw_when_no_parameter_is_provided()
        {
            ((FromUriAttribute)null).Invoking(_ => FromUriAttribute.For(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("parameter");
        }

        [TestMethod]
        public void it_should_create_an_instance_correctly()
        {
            var result = FromUriAttribute.For(Parameter);

            result.Should().BeOfType<FromUriAttribute>().Which.UriTemplate.Should().Be("/{input}");
        }
    }
}