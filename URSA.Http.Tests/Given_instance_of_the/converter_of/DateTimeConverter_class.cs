using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Linq;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [TestClass]
    public class DateTimeConverter_class : ConverterTest<DateTimeConverter>
    {
        [TestMethod]
        public void it_should_deserialize_message_as_an_DateTime()
        {
            DateTime body = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            var result = ConvertTo<DateTime>("POST", "PostString", "text/plain", body.ToString());

            result.Should().Be(body);
        }

        [TestMethod]
        public void it_should_deserialize_message_as_an_array_of_DateTimes()
        {
            DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            DateTime[] body = new[] { now, now.AddMinutes(1) };
            var result = ConvertTo<DateTime[]>("POST", "PostStrings", "text/uri-list", String.Join("\r\n", body));

            result.Should().NotBeNull();
            result.Should().HaveCount(body.Length);
            result.First().Should().Be(body.First());
            result.Last().Should().Be(body.Last());
        }

        [TestMethod]
        public void it_should_serialize_a_DateTime_to_message()
        {
            DateTime body = DateTime.Now;
            var content = ConvertFrom<DateTime>("POST", "PostStrings", "text/plain", body);

            content.Should().Be(body.ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void it_should_serialize_array_of_DateTimes_to_message()
        {
            DateTime[] body = new DateTime[] { DateTime.Now, DateTime.Now.AddHours(1) };
            var content = ConvertFrom<DateTime[]>("POST", "PostStrings", "text/plain", body);

            content.Should().Be(String.Join("\r\n", body.Select(item => item.ToString(CultureInfo.InvariantCulture))));
        }
    }
}