using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class JsonConverter_class : ComplexTypeConverterTest<URSA.Web.Http.Converters.JsonConverter>
    {
        private const string ContentType = "application/json";

        protected override string SingleEntityContentType { get { return ContentType; } }

        protected override string MultipleEntitiesContentType { get { return ContentType; } }

        [Test]
        public override void it_should_not_acknowledge_the_converter_as_a_match_against_incompatible_type_when_deserializing()
        {
        }

        [Test]
        public override void it_should_not_acknowledge_the_converter_as_a_match_against_incompatible_type_when_serializing()
        {
        }

        [Test]
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