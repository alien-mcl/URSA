using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using URSA;
using URSA.Testing;
using URSA.Tests.Web;
using URSA.Web.Mapping;

namespace Given_instance_of
{
    [TestFixture]
    public class FromUrlAttribute_class
    {
        private static readonly ParameterInfo Parameter = typeof(TestController).GetMethod("Result").GetParameters().First();

        [Test]
        public void it_should_throw_when_no_url_is_provided()
        {
            ((FromUrlAttribute)null).Invoking(_ => new FromUrlAttribute(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("url");
        }

        [Test]
        public void it_should_throw_when_url_provided_is_empty()
        {
            ((FromUrlAttribute)null).Invoking(_ => new FromUrlAttribute(String.Empty)).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("url");
        }

        [Test]
        public void it_should_throw_when_url_provided_is_has_no_variable()
        {
            ((FromUrlAttribute)null).Invoking(_ => new FromUrlAttribute("/")).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("url");
        }

        [Test]
        public void it_should_throw_when_no_parameter_is_provided()
        {
            ((FromUrlAttribute)null).Invoking(_ => FromUrlAttribute.For(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("parameter");
        }

        [Test]
        public void it_should_create_an_instance_correctly()
        {
            var result = FromUrlAttribute.For(Parameter);

            result.Should().BeOfType<FromUrlAttribute>().Which.UrlTemplate.ToString().Should().Be("/{input}");
        }

        [SetUp]
        public void Setup()
        {
            UrlParser.Register<RelativeUrlParser>();
        }
    }
}