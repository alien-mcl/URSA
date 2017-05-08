#pragma warning disable 1591 
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RDeF.Entities;
using RDeF.Mapping;
using RDeF.Serialization;
using RDeF.Vocabularies;
using URSA.Configuration;
using URSA.Http.Description.Tests.FluentAssertions;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Testing;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class EntityConverter_class : ConverterTest<EntityConverter, IOperation>
    {
        private const string ContentType = "application/ld+json";
        private const string ClassId = "class";
        private const string Method = "GET";
        private const string BodyPattern = "{{ \"@id\":\"{0}/{2}\", \"@type\":\"{1}Operation\", \"{1}method\":\"GET\", \"{1}returns\":{{ \"@id\":\"{4}/{3}\" }} }},{{ \"@id\":\"{4}/{3}\", \"@type\":\"{1}Class\" }}";
        private static readonly string EntityBody = "[" + String.Format(BodyPattern, BaseUrl, EntityConverter.Hydra, OperationName, ClassId, BaseUrl) + "]";
        private static readonly string EntitiesBody = "[" + String.Format(BodyPattern, BaseUrl, EntityConverter.Hydra, OperationName, ClassId, BaseUrl) +
            "," + String.Format(BodyPattern, BaseUrl, EntityConverter.Hydra, "another", ClassId, BaseUrl) + "]";

        private Mock<IEntityContext> _context;
        private Mock<ISerializableEntitySource> _entitySource;
        private Mock<IOperation> _entity1;
        private Mock<IOperation> _entity2;

        protected override string SingleEntityContentType { get { return ContentType; } }

        protected override string MultipleEntitiesContentType { get { return ContentType; } }

        protected override string SingleEntityBody { get { return EntityBody; } }

        protected override string MultipleEntitiesBody { get { return EntitiesBody; } }

        protected override IOperation SingleEntity { get { return _entity1.Object; } }

        protected override IOperation[] MultipleEntities { get { return new[] { _entity1.Object, _entity2.Object }; } }

        [Test]
        public override void it_should_not_acknowledge_the_converter_as_a_match_against_incompatible_type_when_serializing()
        {
            base.it_should_not_acknowledge_the_converter_as_a_match_against_incompatible_type_when_serializing();
        }

        [Test]
        public override void it_should_throw_when_instance_being_serialized_mismatches_the_converter_supported_type()
        {
            base.it_should_throw_when_instance_being_serialized_mismatches_the_converter_supported_type();
        }

        [Test]
        public override void it_should_do_nothing_if_the_instance_being_serialized_is_null()
        {
            base.it_should_do_nothing_if_the_instance_being_serialized_is_null();
        }

        [Test]
        public override void it_should_not_acknowledge_the_converter_as_a_match_against_incompatible_type_when_deserializing()
        {
            base.it_should_not_acknowledge_the_converter_as_a_match_against_incompatible_type_when_deserializing();
        }

        [Test]
        public override void it_should_test_deserialization_compatibility()
        {
            base.it_should_test_deserialization_compatibility();
        }

        [Test]
        public override void it_should_throw_when_no_expected_type_is_provided_for_deserialization_compatibility_test()
        {
            base.it_should_throw_when_no_expected_type_is_provided_for_deserialization_compatibility_test();
        }

        [Test]
        public override void it_should_throw_when_no_request_is_provided_for_deserialization_compatibility_test()
        {
            base.it_should_throw_when_no_request_is_provided_for_deserialization_compatibility_test();
        }

        [Test]
        public override void it_should_throw_when_no_request_is_provided_for_deserialization()
        {
            base.it_should_throw_when_no_request_is_provided_for_deserialization();
        }

        [Test]
        public override void it_should_throw_when_no_expected_type_is_provided_for_deserialization()
        {
            base.it_should_throw_when_no_expected_type_is_provided_for_deserialization();
        }

        [Test]
        public override void it_should_deserialize_message_body_as_an_entity()
        {
        }

        [Test]
        public override void it_should_deserialize_message_as_an_array_of_entities()
        {
            base.it_should_deserialize_message_as_an_array_of_entities();
        }

        [Test]
        public override void it_should_serialize_array_of_entities_to_message()
        {
            base.it_should_serialize_array_of_entities_to_message();
        }

        [Test]
        public override void it_should_test_serialization_compatibility()
        {
            base.it_should_test_serialization_compatibility();
        }

        [Test]
        public override void it_should_throw_when_no_given_type_is_provided_for_serialization_compatibility_test()
        {
            base.it_should_throw_when_no_given_type_is_provided_for_serialization_compatibility_test();
        }

        [Test]
        public override void it_should_throw_when_no_response_is_provided_for_serialization_compatibility_test()
        {
            base.it_should_throw_when_no_response_is_provided_for_serialization_compatibility_test();
        }

        [Test]
        public override void it_should_throw_when_no_response_is_provided_for_serialization()
        {
            base.it_should_throw_when_no_response_is_provided_for_serialization();
        }

        [Test]
        public override void it_should_throw_when_no_given_type_is_provided_for_serialization()
        {
            base.it_should_throw_when_no_given_type_is_provided_for_serialization();
        }

        [Test]
        public override void it_should_deserialize_message_as_an_entity()
        {
            base.it_should_deserialize_message_as_an_entity();
        }

        [Test]
        public override void it_should_deserialize_message_body_as_an_array_of_entities()
        {
        }

        [Test]
        public override void it_should_serialize_an_entity_to_message()
        {
            base.it_should_serialize_an_entity_to_message();
        }

        [Test]
        public override void it_should_throw_when_no_given_type_is_provided_for_string_deserialization()
        {
        }

        protected override void AssertSingleEntity(IOperation result)
        {
            result.Should().NotBeNull();
            result.Iri.ToString().Should().Be(((Uri)(BaseUrl + ("/" + OperationName))).ToString());
            result.Method.Should().HaveCount(1);
            result.Method.First().Should().Be(Method);
            result.Returns.Should().HaveCount(1);
            result.Returns.First().Iri.ToString().Should().Be(((Uri)(BaseUrl + ClassId)).ToString());
        }

        protected override void AssertSingleEntityMessage(string result)
        {
            ////new JsonLdParser().Load(resultGraph, new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(result))));
            ////new JsonLdParser().Load(expectedGraph, new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(EntityBody))));
            ////resultGraph.Should().BeOfType<Graph>().AndBeEquivalent(expectedGraph);
        }

        protected override void AssertMultipleEntitiesMessage(string result)
        {
            ////new JsonLdParser().Load(resultGraph, new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(result))));
            ////new JsonLdParser().Load(expectedGraph, new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(EntitiesBody))));
            ////resultGraph.Should().BeOfType<Graph>().AndBeEquivalent(expectedGraph);
        }

        protected override EntityConverter CreateInstance()
        {
            _entitySource = new Mock<ISerializableEntitySource>(MockBehavior.Strict);
            _entitySource.Setup(instance => instance.Write(It.IsAny<StreamWriter>(), It.IsAny<IRdfWriter>())).Returns(Task.FromResult(0));
            _context = new Mock<IEntityContext>(MockBehavior.Strict);
            _context.Setup(instance => instance.Load<IOperation>(It.IsAny<Iri>())).Returns<Iri>(id => CreateOperationMock(_entitySource.Object, id).Object);
            _context.Setup(instance => instance.AsQueryable<IOperation>()).Returns(() => MultipleEntities.AsQueryable());
            _context.SetupGet(instance => instance.EntitySource).Returns(_entitySource.Object);
            _entity1 = CreateOperationMock();
            _entity2 = CreateOperationMock("another");
            return new EntityConverter(_context.Object);
        }

        private Mock<IOperation> CreateOperationMock(ISerializableEntitySource entitySource, Iri id)
        {
            var mock = _context.MockEntity<IOperation>(id);
            var method = (from triple in entitySource.Statements
                          where ((triple.Subject == id) && (triple.Object == null) &&
                            (triple.Predicate == new Iri(EntityConverter.Hydra + "method")))
                          select triple.Value).ToList();
            mock.SetupGet(instance => instance.Method).Returns(method);
            var returns = (from triple in entitySource.Statements
                           where ((triple.Subject == id) && (triple.Object != null) &&
                             (triple.Predicate == new Iri(EntityConverter.Hydra + "returns")))
                           select _context.MockEntity<IClass>(triple.Object)).First();
            mock.SetupGet(instance => instance.Returns).Returns(new[] { returns.Object });
            return mock;
        }

        private Mock<IOperation> CreateOperationMock(string operationName = OperationName)
        {
            var statements = CreateOperationMock((Uri)(BaseUrl + ("/" + operationName)), (Uri)(BaseUrl + ClassId));
            var entitySource = new Mock<ISerializableEntitySource>();
            entitySource.Setup(instance => instance.Statements).Returns(statements);
            var context = new Mock<IEntityContext>();
            context.SetupGet(instance => instance.EntitySource).Returns(entitySource.Object);
            var @class = new Mock<IClass>();
            @class.SetupGet(instance => instance.Context).Returns(context.Object);
            var body = new Mock<IOperation>();
            body.SetupGet(instance => instance.Context).Returns(context.Object);
            body.SetupGet(instance => instance.Method).Returns(new List<string>() { Method });
            body.SetupGet(instance => instance.Returns).Returns(new[] { @class.Object });
            return body;
        }

        private IList<Statement> CreateOperationMock(Iri operationUri, Iri classUri)
        {
            IList<Statement> statements = new List<Statement>();
            var triples = new[]
                {
                    new Tuple<Iri, Iri, object>(operationUri, rdf.type, new Iri(EntityConverter.Hydra + "Operation")),
                    new Tuple<Iri, Iri, object>(operationUri, new Iri(EntityConverter.Hydra + "method"), Method),
                    new Tuple<Iri, Iri, object>(operationUri, new Iri(EntityConverter.Hydra + "returns"), classUri),
                    new Tuple<Iri, Iri, object>(classUri, rdf.type, new Iri(EntityConverter.Hydra + "Class"))
                };

            foreach (var triple in triples)
            {
                if (triple.Item3 is Iri)
                {
                    statements.Add(new Statement(triple.Item1, triple.Item2, (Iri)triple.Item3));
                }
                else
                {
                    statements.Add(new Statement(triple.Item1, triple.Item2, (string)triple.Item3));
                }
            }

            return statements;
        }
    }
}