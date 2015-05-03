#pragma warning disable 1591 
using FluentAssertions;
using JsonLD.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using RomanticWeb;
using RomanticWeb.ComponentModel;
using RomanticWeb.Entities;
using RomanticWeb.Vocabularies;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Testing;
using URSA.Web.Http.Testing;
using VDS.RDF;
using EntityQuad = RomanticWeb.Model.EntityQuad;
using Node = RomanticWeb.Model.Node;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class EntityConverter_class : ConverterTest<EntityConverter, IOperation>
    {
        private const string ContentType = "application/ld+json";
        private const string ClassId = "class";
        private const string Method = "GET";
        private const string BodyPattern = "[{{ \"@id\":\"{0}{2}\", \"@type\":\"{1}Operation\", \"{1}method\":\"GET\", \"{1}returns\":{{ \"@id\":\"{0}{3}\" }} }},{{ \"@id\":\"{0}{3}\", \"@type\":\"{1}Class\" }}]";
        private static readonly Uri Hydra = new Uri("http://www.w3.org/ns/hydra/core#");
        private static readonly string Body = String.Format(BodyPattern, BaseUri, Hydra, OperationName, ClassId);
        private Mock<IEntityContext> _context;

        protected override string SingleEntityContentType { get { return ContentType; } }

        protected override string MultipleEntitiesContentType { get { return ContentType; } }

        protected override string SingleEntityBody { get { return Body; } }

        protected override string MultipleEntitiesBody { get { return null; } }

        protected override IOperation SingleEntity { get { return CreateOperationMock().Object; } }

        protected override IOperation[] MultipleEntities { get { return new IOperation[0]; } }

        [TestMethod]
        public override void it_should_test_deserialization_compatibility()
        {
            base.it_should_test_deserialization_compatibility();
        }

        [TestMethod]
        public override void it_should_throw_when_no_expected_type_is_provided_for_deserialization_compatibility_test()
        {
            base.it_should_throw_when_no_expected_type_is_provided_for_deserialization_compatibility_test();
        }

        [TestMethod]
        public override void it_should_throw_when_no_request_is_provided_for_deserialization_compatibility_test()
        {
            base.it_should_throw_when_no_request_is_provided_for_deserialization_compatibility_test();
        }

        [TestMethod]
        public override void it_should_throw_when_no_request_is_provided_for_deserialization()
        {
            base.it_should_throw_when_no_request_is_provided_for_deserialization();
        }

        [TestMethod]
        public override void it_should_throw_when_no_expected_type_is_provided_for_deserialization()
        {
            base.it_should_throw_when_no_expected_type_is_provided_for_deserialization();
        }

        [TestMethod]
        public override void it_should_deserialize_message_as_an_array_of_entities()
        {
        }

        [TestMethod]
        public override void it_should_serialize_array_of_entities_to_message()
        {
        }

        [TestMethod]
        public override void it_should_test_serialization_compatibility()
        {
            base.it_should_test_serialization_compatibility();
        }

        [TestMethod]
        public override void it_should_throw_when_no_given_type_is_provided_for_serialization_compatibility_test()
        {
            base.it_should_throw_when_no_given_type_is_provided_for_serialization_compatibility_test();
        }

        [TestMethod]
        public override void it_should_throw_when_no_response_is_provided_for_serialization_compatibility_test()
        {
            base.it_should_throw_when_no_response_is_provided_for_serialization_compatibility_test();
        }

        [TestMethod]
        public override void it_should_throw_when_no_response_is_provided_for_serialization()
        {
            base.it_should_throw_when_no_response_is_provided_for_serialization();
        }

        [TestMethod]
        public override void it_should_throw_when_no_given_type_is_provided_for_deserialization()
        {
            base.it_should_throw_when_no_given_type_is_provided_for_deserialization();
        }

        [TestMethod]
        public override void it_should_deserialize_message_as_an_entity()
        {
            base.it_should_deserialize_message_as_an_entity();
        }

        [TestMethod]
        public override void it_should_serialize_an_entity_to_message()
        {
            base.it_should_serialize_an_entity_to_message();
        }

        protected override void AssertSingleEntity(IOperation result)
        {
            result.Should().NotBeNull();
            result.Id.ToString().Should().Be(new Uri(BaseUri, OperationName).ToString());
            result.Method.Should().HaveCount(1);
            result.Method.First().Should().Be(Method);
            result.Returns.Should().HaveCount(1);
            result.Returns.First().Id.ToString().Should().Be(new Uri(BaseUri, ClassId).ToString());
        }

        protected override void AssertSingleEntityMessage(string result)
        {
            JsonLdProcessor.Expand(JToken.Parse(result)).ToString().Should().Be(JsonLdProcessor.Expand(JToken.Parse(Body)).ToString());
        }

        protected override EntityConverter CreateInstance()
        {
            ITripleStore tripleStore = null;
            _context = new Mock<IEntityContext>();
            _context.Setup(instance => instance.Load<IOperation>(It.IsAny<EntityId>())).Returns<EntityId>(id => CreateOperationMock(tripleStore, id).Object);
            Mock<IEntityContextFactory> entityContextFactory = new Mock<IEntityContextFactory>();
            entityContextFactory.Setup(instance => instance.CreateContext()).Returns(_context.Object);
            entityContextFactory.As<IComponentRegistryFacade>().Setup(instance => instance.Register(It.IsAny<ITripleStore>()))
                .Callback<ITripleStore>(store => tripleStore = store);
            return new EntityConverter(entityContextFactory.Object);
        }

        private Mock<IOperation> CreateOperationMock(ITripleStore tripleStore, EntityId id)
        {
            var mock = MockHelpers.MockEntity<IOperation>(_context.Object, id);
            var method = (from triple in tripleStore.Triples
                          where (triple.Subject is IUriNode) && (((IUriNode)triple.Subject).Uri.ToString() == id.ToString()) &&
                            (triple.Predicate is IUriNode) && (((IUriNode)triple.Predicate).Uri.ToString() == Hydra + "method") &&
                            (triple.Object is ILiteralNode)
                          select ((ILiteralNode)triple.Object).Value).ToList();
            mock.SetupGet(instance => instance.Method).Returns(method);
            var returns = (from triple in tripleStore.Triples
                           where (triple.Subject is IUriNode) && (((IUriNode)triple.Subject).Uri.ToString() == id.ToString()) &&
                             (triple.Predicate is IUriNode) && (((IUriNode)triple.Predicate).Uri.ToString() == Hydra + "returns") &&
                             (triple.Object is IUriNode)
                           select MockHelpers.MockEntity<IClass>(_context.Object, new EntityId(((IUriNode)triple.Object).Uri))).First();
            mock.SetupGet(instance => instance.Returns).Returns(new IClass[] { returns.Object });
            return mock;
        }

        private Mock<IOperation> CreateOperationMock()
        {
            var operationUri = new Uri(BaseUri, OperationName);
            var operationId = new EntityId(operationUri);
            var classUri = new Uri(BaseUri, ClassId);
            var classId = new EntityId(classUri);
            IList<EntityQuad> quads = new List<EntityQuad>();
            quads.Add(new EntityQuad(operationId, Node.ForUri(operationUri), Node.ForUri(Rdf.type), Node.ForUri(new Uri(Hydra.ToString() + "Operation"))));
            quads.Add(new EntityQuad(operationId, Node.ForUri(operationUri), Node.ForUri(new Uri(Hydra.ToString() + "method")), Node.ForLiteral(Method)));
            quads.Add(new EntityQuad(operationId, Node.ForUri(operationUri), Node.ForUri(new Uri(Hydra.ToString() + "returns")), Node.ForUri(classUri)));
            quads.Add(new EntityQuad(classId, Node.ForUri(classUri), Node.ForUri(Rdf.type), Node.ForUri(new Uri(Hydra.ToString() + "Class"))));
            Mock<IEntityStore> store = new Mock<IEntityStore>();
            store.Setup(instance => instance.Quads).Returns(quads);
            Mock<IEntityContext> context = new Mock<IEntityContext>();
            context.SetupGet(instance => instance.Store).Returns(store.Object);
            Mock<IClass> @class = new Mock<IClass>();
            @class.SetupGet(instance => instance.Context).Returns(context.Object);
            Mock<IOperation> body = new Mock<IOperation>();
            body.SetupGet(instance => instance.Context).Returns(context.Object);
            body.SetupGet(instance => instance.Method).Returns(new List<string>() { Method });
            body.SetupGet(instance => instance.Returns).Returns(new IClass[] { @class.Object });
            return body;
        }
    }
}