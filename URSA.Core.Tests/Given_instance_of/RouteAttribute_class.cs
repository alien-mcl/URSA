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
    public class RouteAttribute_class
    {
        private static readonly ParameterInfo Parameter = typeof(TestController).GetMethod("Result").GetParameters().First();

        [Test]
        public void it_should_throw_when_no_url_is_provided()
        {
            ((RouteAttribute)null).Invoking(_ => new RouteAttribute(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("url");
        }

        [Test]
        public void it_should_throw_when_url_provided_is_empty()
        {
            ((RouteAttribute)null).Invoking(_ => new RouteAttribute(String.Empty)).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("url");
        }

        [Test]
        public void it_should_create_an_instance_correctly()
        {
            var result = new RouteAttribute("/");

            result.Should().BeOfType<RouteAttribute>().Which.Url.ToString().Should().Be("/");
        }

        [SetUp]
        public void Setup()
        {
            UrlParser.Register<RelativeUrlParser>();
        }
    }
}