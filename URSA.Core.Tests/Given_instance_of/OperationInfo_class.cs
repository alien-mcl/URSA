using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Tests.Web;
using URSA.Web.Description;
using URSA.Web.Mapping;

namespace Given_instance_of
{
    [TestClass]
    public class OperationInfo_class
    {
        private static readonly Uri Uri = new Uri("/", UriKind.Relative);
        private static readonly MethodInfo Method = typeof(TestController).GetMethod("Result");
        private static readonly ParameterInfo Result = Method.ReturnParameter;
        private static readonly ParameterInfo Parameter = Method.GetParameters().First();

        [TestMethod]
        public void it_should_throw_when_no_method_is_provided()
        {
            ((FakeOperationInfo)null).Invoking(_ => new FakeOperationInfo(null, Uri, null, null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("underlyingMethod");
        }

        [TestMethod]
        public void it_should_throw_when_provided_underlying_method_is_not_of_the_IController_class()
        {
            ((FakeOperationInfo)null).Invoking(_ => new FakeOperationInfo(GetType().GetMethods().First(), Uri, null, null)).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("underlyingMethod");
        }

        [TestMethod]
        public void it_should_throw_when_no_template_regular_expression_is_passed()
        {
            ((FakeOperationInfo)null).Invoking(_ => new FakeOperationInfo(Method, Uri, null, null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("templateRegex");
        }

        [TestMethod]
        public void it_should_acknowledge_two_objects_to_be_different()
        {
            var leftOperand = new OperationInfo<string>(Method, Uri, "/", new Regex(".*"), "test");

            leftOperand.Equals(null).Should().BeFalse();
            leftOperand.Equals(String.Empty).Should().BeFalse();
        }

        [TestMethod]
        public void it_should_acknowledge_two_operations_as_equal()
        {
            var leftOperand = new OperationInfo<string>(Method, Uri, "/", new Regex(".*"), "test");
            var rightOperand = new OperationInfo<string>(Method, Uri, "/", new Regex(".*"), "test");

            (leftOperand == rightOperand).Should().BeTrue();
            leftOperand.Equals(leftOperand).Should().BeTrue();
        }

        [TestMethod]
        public void it_should_acknowledge_two_operations_as_inequal()
        {
            var leftOperand = new OperationInfo<string>(Method, Uri, "/", new Regex(".*"), "test");
            var rightOperand = new OperationInfo<string>(Method, new Uri("/test", UriKind.Relative), "/", new Regex(".*"), "test");

            (leftOperand != rightOperand).Should().BeTrue();
        }

        [TestMethod]
        public void it_should_create_an_instance_correctly()
        {
            var result = new FakeOperationInfo(
                Method,
                new Uri("/", UriKind.Relative),
                "/",
                new Regex(".*"),
                new ResultInfo(Result, new ToBodyAttribute(), "/", "test"),
                new ArgumentInfo(Parameter, new FromUriAttribute(), "/", "test"));

            result.Should().BeOfType<FakeOperationInfo>();
        }

        private class FakeOperationInfo : OperationInfo
        {
            public FakeOperationInfo(MethodInfo underlyingMethod, Uri uri, string uriTemplate, Regex templateRegex, params ValueInfo[] values)
                : base(underlyingMethod, uri, uriTemplate, templateRegex, values)
            {
            }
        }
    }
}