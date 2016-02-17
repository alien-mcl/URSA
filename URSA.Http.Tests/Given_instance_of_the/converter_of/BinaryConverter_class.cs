using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Converters;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class BinaryConverter_class : ConverterTest<BinaryConverter, byte>
    {
        private const string ContentType = "application/octet-stream";
        private const byte Entity = default(byte);
        private static readonly byte[] Entities = { 0x01, 0x02 };

        protected override string SingleEntityContentType { get { return ContentType; } }

        protected override string MultipleEntitiesContentType { get { return ContentType; } }

        protected override byte SingleEntity { get { return Entity; } }

        protected override byte[] MultipleEntities { get { return Entities; } }

        protected override string MultipleEntitiesBody { get { return System.Convert.ToBase64String(Entities); } }

        [TestMethod]
        public override void it_should_test_deserialization_compatibility()
        {
            var result = CanConvertTo<byte[]>("POST", OperationName, MultipleEntitiesContentType, MultipleEntitiesBody);

            result.Should().NotBe(CompatibilityLevel.None);
        }

        [TestMethod]
        public override void it_should_test_serialization_compatibility()
        {
            var result = CanConvertFrom("POST", OperationName, MultipleEntitiesContentType, MultipleEntities);

            result.Should().NotBe(CompatibilityLevel.None);
        }

        [TestMethod]
        public override void it_should_deserialize_message_as_an_entity()
        {
        }

        [TestMethod]
        public override void it_should_serialize_an_entity_to_message()
        {
        }

        [TestMethod]
        public override void it_should_deserialize_message_body_as_an_entity()
        {
        }
    }
}