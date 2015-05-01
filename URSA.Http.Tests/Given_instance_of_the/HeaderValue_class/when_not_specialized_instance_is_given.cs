using System;
using System.Diagnostics.CodeAnalysis;
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