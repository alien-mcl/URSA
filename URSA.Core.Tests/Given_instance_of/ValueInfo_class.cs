using System;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using URSA.Tests.Web;
using URSA.Web.Description;

namespace Given_instance_of
{
    [TestFixture]
    public class ValueInfo_class
    {
        private static readonly ParameterInfo Parameter = typeof(TestController).GetMethod("Result").ReturnParameter;

        [Test]
        public void it_should_throw_when_no_parameter_is_provided()
        {
            ((FakeValueInfo)null).Invoking(_ => new FakeValueInfo(null, null, null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("parameter");
        }

        [Test]
        public void it_should_throw_when_no_template_is_provided_with_variable_name_passed()
        {
            ((FakeValueInfo)null).Invoking(_ => new FakeValueInfo(Parameter, null, "test")).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("urlTemplate");
        }

        [Test]
        public void it_should_throw_when_template_provided_is_empty_with_variable_name_passed()
        {
            ((FakeValueInfo)null).Invoking(_ => new FakeValueInfo(Parameter, String.Empty, "test")).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("urlTemplate");
        }

        [Test]
        public void it_should_throw_when_no_variable_name_is_provided_with_tempate_url_passed()
        {
            ((FakeValueInfo)null).Invoking(_ => new FakeValueInfo(Parameter, "test", null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("variableName");
        }

        [Test]
        public void it_should_throw_when_variable_name_provided_is_empty_with_tempate_url_passed()
        {
            ((FakeValueInfo)null).Invoking(_ => new FakeValueInfo(Parameter, "test", String.Empty)).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("variableName");
        }

        [Test]
        public void it_should_create_an_instance_correctly()
        {
            var result = new FakeValueInfo(Parameter, "test", "test");

            result.Should().BeOfType<FakeValueInfo>();
        }

        private class FakeValueInfo : ValueInfo
        {
            public FakeValueInfo(ParameterInfo parameter, string urlTemplate, string variableName) : base(parameter, urlTemplate, variableName)
            {
            }
        }
    }
}