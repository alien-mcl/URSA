using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA;
using URSA.Testing;
using URSA.Tests.Web;
using URSA.Web.Mapping;

namespace Given_instance_of
{
    [TestClass]
    public class RouteAttribute_class
    {
        private static readonly ParameterInfo Parameter = typeof(TestController).GetMethod("Result").GetParameters().First();

        [TestMethod]
        public void it_should_throw_when_no_url_is_provided()
        {
            ((RouteAttribute)null).Invoking(_ => new RouteAttribute(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("url");
        }

        [TestMethod]
        public void it_should_throw_when_url_provided_is_empty()
        {
            ((RouteAttribute)null).Invoking(_ => new RouteAttribute(String.Empty)).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("url");
        }

        [TestMethod]
        public void it_should_create_an_instance_correctly()
        {
            var result = new RouteAttribute("/");

            result.Should().BeOfType<RouteAttribute>().Which.Url.ToString().Should().Be("/");
        }

        [TestInitialize]
        public void Setup()
        {
            UrlParser.Register<RelativeUrlParser>();
        }
    }
}