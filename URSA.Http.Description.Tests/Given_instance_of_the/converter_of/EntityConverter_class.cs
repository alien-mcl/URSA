#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using RDeF.Entities;
using RDeF.Serialization;
using RDeF.Vocabularies;
using URSA.Web.Http.Converters;
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

        private List<Statement> _statements;
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
        public override void it_should_deserialize_message_body_as_an_entity()
        {
        }

        [Test]
        public override void it_should_deserialize_message_body_as_an_array_of_entities()
        {
        }

        [Test]
        public override void it_should_throw_when_no_given_type_is_provided_for_string_deserialization()
        {
        }

        protected override void AssertSingleEntity(IOperation result)
        {
            result.Should().NotBeNull();
            result.Iri.ToString().Should().Be(((Uri)(BaseUrl + ("/" + OperationName))).ToString());
            result.Method.Should().HaveCount(2);
            result.Method.First().Should().Be(Method);
            result.Returns.Should().HaveCount(1);
            result.Returns.First().Iri.ToString().Should().Be(((Uri)(BaseUrl + ClassId)).ToString());
        }

        protected override void AssertSingleEntityMessage(string result)
        {
            AssertMultipleEntitiesMessage(result);
        }

        protected override void AssertMultipleEntitiesMessage(string result)
        {
            var resultingStatements = new JsonLdReader().Read(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(result)))).Result;
            var expectedStatements = new JsonLdReader().Read(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(EntitiesBody)))).Result;
            resultingStatements.ShouldBeEquivalentTo(expectedStatements);
        }

        protected override EntityConverter CreateInstance()
        {
            _statements = new List<Statement>();
            _entitySource = new Mock<ISerializableEntitySource>(MockBehavior.Strict);
            _entitySource.SetupGet(instance => instance.Statements).Returns(_statements);
            _entitySource.Setup(instance => instance.Write(It.IsAny<StreamWriter>(), It.IsAny<IRdfWriter>()))
                .Callback<StreamWriter, IRdfWriter>((streamWriter, rdfWriter) => rdfWriter.Write(streamWriter, new[] { new KeyValuePair<Iri, IEnumerable<Statement>>(null, _statements) }))
                .Returns(Task.FromResult(0));
            _entitySource.Setup(instance => instance.Read(It.IsAny<StreamReader>(), It.IsAny<IRdfReader>()))
                .Callback<StreamReader, IRdfReader>((streamReader, rdfReader) => _statements.AddRange(rdfReader.Read(streamReader).Result.SelectMany(graph => graph.Value)))
                .Returns(Task.FromResult(0));
            _context = new Mock<IEntityContext>(MockBehavior.Strict);
            _context.Setup(instance => instance.Load<IOperation>(It.IsAny<Iri>())).Returns<Iri>(id => CreateOperationMock(id).Object);
            _context.Setup(instance => instance.AsQueryable<IOperation>()).Returns(() => MultipleEntities.AsQueryable());
            _context.SetupGet(instance => instance.EntitySource).Returns(_entitySource.Object);
            _entity1 = CreateOperationMock();
            _entity2 = CreateOperationMock("another");
            return new EntityConverter(_context.Object);
        }

        private Mock<IOperation> CreateOperationMock(Iri id)
        {
            var mock = _context.MockEntity<IOperation>(id);
            var method = (from triple in _statements
                          where ((triple.Subject == id) && (triple.Object == null) &&
                            (triple.Predicate == new Iri(EntityConverter.Hydra + "method")))
                          select triple.Value).ToList();
            mock.SetupGet(instance => instance.Method).Returns(method);
            var returns = (from triple in _statements
                           where ((triple.Subject == id) && (triple.Object != null) &&
                             (triple.Predicate == new Iri(EntityConverter.Hydra + "returns")))
                           select _context.MockEntity<IClass>(triple.Object)).First();
            mock.SetupGet(instance => instance.Returns).Returns(new[] { returns.Object });
            return mock;
        }

        private Mock<IOperation> CreateOperationMock(string operationName = OperationName)
        {
            _statements.AddRange(CreateOperationMock((Uri)(BaseUrl + ("/" + operationName)), (Uri)(BaseUrl + ClassId)));
            var @class = new Mock<IClass>();
            @class.SetupGet(instance => instance.Context).Returns(_context.Object);
            var body = new Mock<IOperation>();
            body.SetupGet(instance => instance.Context).Returns(_context.Object);
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