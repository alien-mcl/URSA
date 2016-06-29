#pragma warning disable 1591 
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RomanticWeb;
using RomanticWeb.Entities;
using RomanticWeb.Vocabularies;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using RomanticWeb.Configuration;
using RomanticWeb.Mapping.Model;
using RomanticWeb.NamedGraphs;
using URSA.Configuration;
using URSA.Http.Description.Tests.FluentAssertions;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Testing;
using URSA.Web.Http.Description.VDS.RDF;
using URSA.Web.Http.Testing;
using VDS.RDF;
using VDS.RDF.Parsing;
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
        private const string BodyPattern = "{{ \"@id\":\"{0}/{2}\", \"@type\":\"{1}Operation\", \"{1}method\":\"GET\", \"{1}returns\":{{ \"@id\":\"{4}/{3}\" }} }},{{ \"@id\":\"{4}/{3}\", \"@type\":\"{1}Class\" }}";
        private static readonly string EntityBody = "[" + String.Format(BodyPattern, BaseUrl, EntityConverter.Hydra, OperationName, ClassId, BaseUrl) + "]";
        private static readonly string EntitiesBody = "[" + String.Format(BodyPattern, BaseUrl, EntityConverter.Hydra, OperationName, ClassId, BaseUrl) +
            "," + String.Format(BodyPattern, BaseUrl, EntityConverter.Hydra, "another", ClassId, BaseUrl) + "]";

        private static readonly Uri MetaGraphUri = ConfigurationSectionHandler.Default.Factories[DescriptionConfigurationSection.Default.DefaultStoreFactoryName].MetaGraphUri;

        private Mock<IEntityContext> _context;
        private ITripleStore _tripleStore;
        private Mock<IOperation> _entity1;
        private Mock<IOperation> _entity2;

        protected override string SingleEntityContentType { get { return ContentType; } }

        protected override string MultipleEntitiesContentType { get { return ContentType; } }

        protected override string SingleEntityBody { get { return EntityBody; } }

        protected override string MultipleEntitiesBody { get { return EntitiesBody; } }

        protected override IOperation SingleEntity { get { return _entity1.Object; } }

        protected override IOperation[] MultipleEntities { get { return new[] { _entity1.Object, _entity2.Object }; } }

        [TestMethod]
        public override void it_should_not_acknowledge_the_converter_as_a_match_against_incompatible_type_when_serializing()
        {
            base.it_should_not_acknowledge_the_converter_as_a_match_against_incompatible_type_when_serializing();
        }

        [TestMethod]
        public override void it_should_throw_when_instance_being_serialized_mismatches_the_converter_supported_type()
        {
            base.it_should_throw_when_instance_being_serialized_mismatches_the_converter_supported_type();
        }

        [TestMethod]
        public override void it_should_do_nothing_if_the_instance_being_serialized_is_null()
        {
            base.it_should_do_nothing_if_the_instance_being_serialized_is_null();
        }

        [TestMethod]
        public override void it_should_not_acknowledge_the_converter_as_a_match_against_incompatible_type_when_deserializing()
        {
            base.it_should_not_acknowledge_the_converter_as_a_match_against_incompatible_type_when_deserializing();
        }

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
        public override void it_should_deserialize_message_body_as_an_entity()
        {
        }

        [TestMethod]
        public override void it_should_deserialize_message_as_an_array_of_entities()
        {
            base.it_should_deserialize_message_as_an_array_of_entities();
        }

        [TestMethod]
        public override void it_should_serialize_array_of_entities_to_message()
        {
            base.it_should_serialize_array_of_entities_to_message();
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
        public override void it_should_throw_when_no_given_type_is_provided_for_serialization()
        {
            base.it_should_throw_when_no_given_type_is_provided_for_serialization();
        }

        [TestMethod]
        public override void it_should_deserialize_message_as_an_entity()
        {
            base.it_should_deserialize_message_as_an_entity();
        }

        [TestMethod]
        public override void it_should_deserialize_message_body_as_an_array_of_entities()
        {
        }

        [TestMethod]
        public override void it_should_serialize_an_entity_to_message()
        {
            var targetGraph = _tripleStore.Graphs.First(graph => !AbsoluteUriComparer.Default.Equals(graph.BaseUri, MetaGraphUri));
            targetGraph.Retract(targetGraph.Triples.Where(triple => ((IUriNode)triple.Subject).Uri == (Uri)(BaseUrl + ("/another"))));
            base.it_should_serialize_an_entity_to_message();
        }

        [TestMethod]
        public override void it_should_throw_when_no_given_type_is_provided_for_string_deserialization()
        {
        }

        protected override void AssertSingleEntity(IOperation result)
        {
            result.Should().NotBeNull();
            result.Id.ToString().Should().Be(((Uri)(BaseUrl + ("/" + OperationName))).ToString());
            result.Method.Should().HaveCount(1);
            result.Method.First().Should().Be(Method);
            result.Returns.Should().HaveCount(1);
            result.Returns.First().Id.ToString().Should().Be(((Uri)(BaseUrl + ClassId)).ToString());
        }

        protected override void AssertSingleEntityMessage(string result)
        {
            var resultGraph = new Graph();
            new JsonLdParser().Load(resultGraph, new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(result))));
            var expectedGraph = new Graph();
            new JsonLdParser().Load(expectedGraph, new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(EntityBody))));
            resultGraph.Should().BeOfType<Graph>().AndBeEquivalent(expectedGraph);
        }

        protected override void AssertMultipleEntitiesMessage(string result)
        {
            var resultGraph = new Graph();
            new JsonLdParser().Load(resultGraph, new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(result))));
            var expectedGraph = new Graph();
            new JsonLdParser().Load(expectedGraph, new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(EntitiesBody))));
            resultGraph.Should().BeOfType<Graph>().AndBeEquivalent(expectedGraph);
        }

        protected override EntityConverter CreateInstance()
        {
            _tripleStore = new TripleStore();
            _tripleStore.FindOrCreate(MetaGraphUri);
            _context = new Mock<IEntityContext>(MockBehavior.Strict);
            _context.Setup(instance => instance.Load<IOperation>(It.IsAny<EntityId>())).Returns<EntityId>(id => CreateOperationMock(_tripleStore, id).Object);
            _context.Setup(instance => instance.AsQueryable<IOperation>()).Returns(() => MultipleEntities.AsQueryable());
            var entityContextProvider = new Mock<IEntityContextProvider>(MockBehavior.Strict);
            entityContextProvider.SetupGet(instance => instance.EntityContext).Returns(_context.Object);
            entityContextProvider.SetupGet(instance => instance.TripleStore).Returns(_tripleStore);
            entityContextProvider.SetupGet(instance => instance.MetaGraph).Returns(MetaGraphUri);
            var namedGraphSelector = new Mock<INamedGraphSelector>(MockBehavior.Strict);
            namedGraphSelector.Setup(instance => instance.SelectGraph(It.IsAny<EntityId>(), It.IsAny<IEntityMapping>(), It.IsAny<IPropertyMapping>())).Returns((Uri)BaseUrl);
            _entity1 = CreateOperationMock();
            _entity2 = CreateOperationMock("another");
            return new EntityConverter(entityContextProvider.Object, namedGraphSelector.Object);
        }

        private Mock<IOperation> CreateOperationMock(ITripleStore tripleStore, EntityId id)
        {
            var mock = _context.MockEntity<IOperation>(id);
            var method = (from triple in tripleStore.Triples
                          where (triple.Subject is IUriNode) && (((IUriNode)triple.Subject).Uri.ToString() == id.ToString()) &&
                            (triple.Predicate is IUriNode) && (((IUriNode)triple.Predicate).Uri.ToString() == EntityConverter.Hydra + "method") &&
                            (triple.Object is ILiteralNode)
                          select ((ILiteralNode)triple.Object).Value).ToList();
            mock.SetupGet(instance => instance.Method).Returns(method);
            var returns = (from triple in tripleStore.Triples
                           where (triple.Subject is IUriNode) && (((IUriNode)triple.Subject).Uri.ToString() == id.ToString()) &&
                             (triple.Predicate is IUriNode) && (((IUriNode)triple.Predicate).Uri.ToString() == EntityConverter.Hydra + "returns") &&
                             (triple.Object is IUriNode)
                           select _context.MockEntity<IClass>(new EntityId(((IUriNode)triple.Object).Uri))).First();
            mock.SetupGet(instance => instance.Returns).Returns(new[] { returns.Object });
            return mock;
        }

        private Mock<IOperation> CreateOperationMock(string operationName = OperationName)
        {
            var quads = CreateOperationMock((Uri)(BaseUrl + ("/" + operationName)), (Uri)(BaseUrl + ClassId));
            var store = new Mock<IEntityStore>();
            store.Setup(instance => instance.Quads).Returns(quads);
            var context = new Mock<IEntityContext>();
            context.SetupGet(instance => instance.Store).Returns(store.Object);
            var @class = new Mock<IClass>();
            @class.SetupGet(instance => instance.Context).Returns(context.Object);
            var body = new Mock<IOperation>();
            body.SetupGet(instance => instance.Context).Returns(context.Object);
            body.SetupGet(instance => instance.Method).Returns(new List<string>() { Method });
            body.SetupGet(instance => instance.Returns).Returns(new[] { @class.Object });
            return body;
        }

        private IList<EntityQuad> CreateOperationMock(Uri operationUri, Uri classUri)
        {
            IList<EntityQuad> quads = new List<EntityQuad>();
            var graph = _tripleStore.FindOrCreate((Uri)BaseUrl);
            var triples = new[]
                {
                    new Tuple<Uri, Uri, object>(operationUri, Rdf.type, new Uri(EntityConverter.Hydra + "Operation")),
                    new Tuple<Uri, Uri, object>(operationUri, new Uri(EntityConverter.Hydra + "method"), Method),
                    new Tuple<Uri, Uri, object>(operationUri, new Uri(EntityConverter.Hydra + "returns"), classUri),
                    new Tuple<Uri, Uri, object>(classUri, Rdf.type, new Uri(EntityConverter.Hydra + "Class"))
                };

            foreach (var triple in triples)
            {
                if (triple.Item3 is Uri)
                {
                    graph.Assert(graph.CreateUriNode(triple.Item1), graph.CreateUriNode(triple.Item2), graph.CreateUriNode((Uri)triple.Item3));
                }
                else
                {
                    graph.Assert(graph.CreateUriNode(triple.Item1), graph.CreateUriNode(triple.Item2), graph.CreateLiteralNode(triple.Item3.ToString()));
                }

                var value = (triple.Item3 is Uri ? Node.ForUri((Uri)triple.Item3) : Node.ForLiteral(triple.Item3.ToString()));
                quads.Add(new EntityQuad(new EntityId(triple.Item1), Node.ForUri(triple.Item1), Node.ForUri(triple.Item2), value));
            }

            return quads;
        }
    }
}