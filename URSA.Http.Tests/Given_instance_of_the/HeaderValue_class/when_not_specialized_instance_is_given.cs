using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Http;

namespace Given_instance_of_the.HeaderValue_class
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class when_not_specialized_instance_is_given
    {
        private const string Value = "text/plain";
        private const string ParameterName = "charset";
        private const string ParameterValue = "utf-8";

        private HeaderValue _headerValue;

        [TestMethod]
        public void it_should_try_and_parse_a_header_value()
        {
            HeaderValue value;
            HeaderValue.TryParse("test;whatever=\"whatever value\n \\x\\; I could imagine\"; another=\"1\"", out value).Should().BeTrue();
            value.Parameters.Should().HaveCount(2).And.Subject.Last().Name.Should().Be("another");
            value.Parameters["whatever"].Should().BeOfType<HeaderParameter>().Which.Value.Should().Be("whatever value\n \\x\\; I could imagine");
        }

        [TestMethod]
        public void it_should_not_try_and_parse_a_header_value()
        {
            HeaderValue value;
            HeaderValue.TryParse(null, out value).Should().BeFalse();
        }

        [TestMethod]
        public void it_should_throw_when_no_header_value_is_passed_for_parsing()
        {
            ((HeaderValue)null).Invoking(_ => HeaderValue.Parse(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("value");
        }

        [TestMethod]
        public void it_should_parse_header_value()
        {
            _headerValue.Should().NotBeNull();
            _headerValue.Value.Should().Be(Value);
        }

        [TestMethod]
        public void it_should_parse_header_value_parameters()
        {
            _headerValue.Parameters.Should().HaveCount(1);
        }

        [TestMethod]
        public void it_should_detect_that_one_value_equals_another()
        {
            _headerValue.Equals(new HeaderValue(Value, new HeaderParameter(ParameterName, ParameterValue))).Should().BeTrue();
        }

        [TestMethod]
        public void it_should_detect_that_two_values_are_equal()
        {
            (_headerValue == String.Format("{0}; {1}={2}", Value, ParameterName, ParameterValue)).Should().BeTrue();
        }

        [TestMethod]
        public void it_should_detect_that_two_values_are_unequal()
        {
            (_headerValue != Value).Should().BeTrue();
        }

        [TestMethod]
        public void it_should_not_consider_two_different_values_as_equal()
        {
            (new HeaderValue("test", new HeaderParameter("test", 1)) == "test;q=\"1\"").Should().BeFalse();
        }

        [TestMethod]
        public void it_should_not_consider_two_different_objects_as_equal()
        {
            new HeaderValue("test").Equals("test").Should().BeFalse();
        }

        [TestMethod]
        public void it_should_consider_same_instance_as_equal()
        {
            var value = new HeaderValue("test");
            value.Equals(value).Should().BeTrue();
        }

        [TestInitialize]
        public void Setup()
        {
            HeaderValue.TryParse(String.Format("{0}; {1}={2}", Value, ParameterName, ParameterValue), out _headerValue);
        }

        [TestCleanup]
        public void Teardown()
        {
            _headerValue = null;
        }
    }
}