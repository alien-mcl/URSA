using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class XWwwUrlEncodedConverter_class : ComplexTypeConverterTest<URSA.Web.Http.Converters.XWwwUrlEncodedConverter>
    {
        private const string ContentType = "application/x-www-url-encoded";

        protected override string SingleEntityContentType { get { return ContentType; } }

        protected override string MultipleEntitiesContentType { get { return ContentType; } }

        protected override bool SupportsMultipleInstances { get { return false; } }

        [TestMethod]
        public override void it_should_not_acknowledge_the_converter_as_a_match_against_incompatible_type_when_deserializing()
        {
        }

        [TestMethod]
        public override void it_should_not_acknowledge_the_converter_as_a_match_against_incompatible_type_when_serializing()
        {
        }

        protected override string SerializeObject<TI>(TI obj)
        {
            StringBuilder result = new StringBuilder(1024);
            string separator = String.Empty;
            foreach (var property in obj.GetType().GetTypeInfo().GetProperties())
            {
                var value = property.GetValue(obj);
                if (value == null)
                {
                    continue;
                }

                if ((value is IEnumerable) && (!(value is string)))
                {
                    foreach (var item in (IEnumerable)value)
                    {
                        result.AppendFormat("{0}{1}={2}", separator, property.Name, item.ToString().UrlEncode());
                        separator = "&";
                    }
                }
                else
                {
                    result.AppendFormat("{0}{1}={2}", separator, property.Name, value.ToString().UrlEncode());
                }

                separator = "&";
            }

            return result.ToString();
        }
    }
}