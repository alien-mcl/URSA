using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class JsonConverter_class : ComplexTypeConverterTest<URSA.Web.Http.Converters.JsonConverter>
    {
        private const string ContentType = "application/json";

        protected override string SingleEntityContentType { get { return ContentType; } }

        protected override string MultipleEntitiesContentType { get { return ContentType; } }

        [TestMethod]
        public override void it_should_not_acknowledge_the_converter_as_a_match_against_incompatible_type_when_deserializing()
        {
        }

        [TestMethod]
        public override void it_should_not_acknowledge_the_converter_as_a_match_against_incompatible_type_when_serializing()
        {
        }

        [TestMethod]
        public override void it_should_do_nothing_if_the_instance_being_serialized_is_null()
        {
            ConvertFrom("POST", OperationName, SingleEntityContentType, null).Should().Be("null");
        }

        protected override string SerializeObject<TI>(TI obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}