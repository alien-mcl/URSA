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
    public class NumberConverter_class : ConverterTest<NumberConverter>
    {
        [TestMethod]
        public void it_should_deserialize_message_as_an_int()
        {
            int body = 1;
            var result = ConvertTo<int>("POST", "PostString", "text/plain", body.ToString());

            result.Should().Be(body);
        }

        [TestMethod]
        public void it_should_deserialize_message_as_an_array_of_doubles()
        {
            double[] body = new[] { 1.0, 2.0 };
            var result = ConvertTo<double[]>("POST", "PostStrings", "text/uri-list", String.Join("\r\n", body));

            result.Should().NotBeNull();
            result.Should().HaveCount(body.Length);
            result.First().Should().Be(body.First());
            result.Last().Should().Be(body.Last());
        }

        [TestMethod]
        public void it_should_serialize_an_int_to_message()
        {
            int body = 1;
            var content = ConvertFrom<int>("POST", "PostStrings", "text/plain", body);

            content.Should().Be(body.ToString());
        }

        [TestMethod]
        public void it_should_serialize_array_of_decimals_to_message()
        {
            decimal[] body = new decimal[] { 1.0M, 2.0M };
            var content = ConvertFrom<decimal[]>("POST", "PostStrings", "text/plain", body);

            content.Should().Be(String.Join("\r\n", body.Select(item => item.ToString(CultureInfo.InvariantCulture))));
        }
    }
}