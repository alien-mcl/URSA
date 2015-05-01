using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Http;

namespace Given_instance_of_the.HeaderValue_class
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class when_integer_specialized_instance_is_given
    {
        private const int Value = 123;
        private HeaderValue<int> _headerValue;

        [TestMethod]
        public void it_should_parse_header_value()
        {
            _headerValue.Should().NotBeNull();
            _headerValue.Value.Should().Be(Value);
        }

        [TestMethod]
        public void it_should_convert_strongly_typed_value_back_to_string()
        {
            int newValue = 5;

            _headerValue.Value = newValue;

            ((HeaderValue)_headerValue).Value.Should().Be(newValue.ToString());
        }

        [TestInitialize]
        public void Setup()
        {
            _headerValue = (HeaderValue<int>)HeaderValue.ParseInternal("Content-Length", Value.ToString());
        }

        [TestCleanup]
        public void Teardown()
        {
            _headerValue = null;
        }
    }
}