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
    public class RouteAttribute_class
    {
        private static readonly ParameterInfo Parameter = typeof(TestController).GetMethod("Result").GetParameters().First();

        [TestMethod]
        public void it_should_throw_when_no_uri_is_provided()
        {
            ((RouteAttribute)null).Invoking(_ => new RouteAttribute(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("uri");
        }

        [TestMethod]
        public void it_should_throw_when_uri_provided_is_empty()
        {
            ((RouteAttribute)null).Invoking(_ => new RouteAttribute(String.Empty)).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("uri");
        }

        [TestMethod]
        public void it_should_create_an_instance_correctly()
        {
            var result = new RouteAttribute("/");

            result.Should().BeOfType<RouteAttribute>().Which.Uri.ToString().Should().Be("/");
        }
    }
}