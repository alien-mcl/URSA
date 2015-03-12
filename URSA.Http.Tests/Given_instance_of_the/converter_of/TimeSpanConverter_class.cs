using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TimeSpanConverter_class : ConverterTest<TimeSpanConverter>
    {
        [TestMethod]
        public void it_should_deserialize_message_as_an_TimeSpan()
        {
            TimeSpan body = DateTime.Now.TimeOfDay;
            var result = ConvertTo<TimeSpan>("POST", "PostString", "text/plain", body.ToString());

            result.Should().Be(body);
        }

        [TestMethod]
        public void it_should_deserialize_message_as_an_array_of_TimeSpans()
        {
            TimeSpan now = DateTime.Now.TimeOfDay;
            TimeSpan[] body = new[] { now, now + now };
            var result = ConvertTo<TimeSpan[]>("POST", "PostStrings", "text/uri-list", String.Join("\r\n", body));

            result.Should().NotBeNull();
            result.Should().HaveCount(body.Length);
            result.First().Should().Be(body.First());
            result.Last().Should().Be(body.Last());
        }

        [TestMethod]
        public void it_should_serialize_a_TimeSpan_to_message()
        {
            TimeSpan body = DateTime.Now.TimeOfDay;
            var content = ConvertFrom<TimeSpan>("POST", "PostStrings", "text/plain", body);

            content.Should().Be(body.ToString());
        }

        [TestMethod]
        public void it_should_serialize_array_of_TimeSpans_to_message()
        {
            TimeSpan[] body = new TimeSpan[] { DateTime.Now.TimeOfDay, TimeSpan.FromHours(1) };
            var content = ConvertFrom<TimeSpan[]>("POST", "PostStrings", "text/plain", body);

            content.Should().Be(String.Join("\r\n", body.Select(item => item.ToString())));
        }
    }
}