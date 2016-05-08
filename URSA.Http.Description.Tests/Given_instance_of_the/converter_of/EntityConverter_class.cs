#pragma warning disable 1591 
using FluentAssertions;
using JsonLD.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using RomanticWeb;
using RomanticWeb.Entities;
using RomanticWeb.Vocabularies;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RomanticWeb.Configuration;
using RomanticWeb.Mapping.Model;
using RomanticWeb.NamedGraphs;
using URSA.Configuration;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.NamedGraphs;
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
        private const string BodyPattern = "[{{ \"@id\":\"{0}/{2}\", \"@type\":\"{1}Operation\", \"{1}method\":\"GET\", \"{1}returns\":{{ \"@id\":\"{0}/{3}\" }} }},{{ \"@id\":\"{0}/{3}\", \"@type\":\"{1}Class\" }}]";
        private static readonly string Body = String.Format(BodyPattern, BaseUrl, EntityConverter.Hydra, OperationName, ClassId);
        private Mock<IEntityContext> _context;
        private ITripleStore _tripleStore;

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
        public override void it_should_serialize_an_entity_to_message()
        {
            base.it_should_serialize_an_entity_to_message();
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
            JsonLdProcessor.Expand(JToken.Parse(result)).ToString().Should().Be(JsonLdProcessor.Expand(JToken.Parse(Body)).ToString());
        }

        protected override EntityConverter CreateInstance()
        {
            _tripleStore = new TripleStore();
            var metaGraph = new Graph() { BaseUri = ConfigurationSectionHandler.Default.Factories[DescriptionConfigurationSection.Default.DefaultStoreFactoryName].MetaGraphUri };
            _tripleStore.Add(metaGraph);
            _context = new Mock<IEntityContext>();
            _context.Setup(instance => instance.Load<IOperation>(It.IsAny<EntityId>())).Returns<EntityId>(id => CreateOperationMock(_tripleStore, id).Object);
            var entityContextProvider = new Mock<IEntityContextProvider>(MockBehavior.Strict);
            entityContextProvider.SetupGet(instance => instance.EntityContext).Returns(_context.Object);
            entityContextProvider.SetupGet(instance => instance.TripleStore).Returns(_tripleStore);
            var namedGraphSelector = new Mock<INamedGraphSelector>(MockBehavior.Strict);
            namedGraphSelector.Setup(instance => instance.SelectGraph(It.IsAny<EntityId>(), It.IsAny<IEntityMapping>(), It.IsAny<IPropertyMapping>())).Returns((Uri)BaseUrl);
            var namedGraphSelectorFactory = new Mock<INamedGraphSelectorFactory>(MockBehavior.Strict);
            namedGraphSelectorFactory.SetupGet(instance => instance.NamedGraphSelector).Returns(namedGraphSelector.Object);
            return new EntityConverter(entityContextProvider.Object, namedGraphSelectorFactory.Object);
        }

        private Mock<IOperation> CreateOperationMock(ITripleStore tripleStore, EntityId id)
        {
            var mock = MockHelpers.MockEntity<IOperation>(_context.Object, id);
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
                           select MockHelpers.MockEntity<IClass>(_context.Object, new EntityId(((IUriNode)triple.Object).Uri))).First();
            mock.SetupGet(instance => instance.Returns).Returns(new[] { returns.Object });
            return mock;
        }

        private Mock<IOperation> CreateOperationMock()
        {
            var quads = CreateOperationMock((Uri)(BaseUrl + ("/" + OperationName)), (Uri)(BaseUrl + ClassId));
            Mock<IEntityStore> store = new Mock<IEntityStore>();
            store.Setup(instance => instance.Quads).Returns(quads);
            Mock<IEntityContext> context = new Mock<IEntityContext>();
            context.SetupGet(instance => instance.Store).Returns(store.Object);
            Mock<IClass> @class = new Mock<IClass>();
            @class.SetupGet(instance => instance.Context).Returns(context.Object);
            Mock<IOperation> body = new Mock<IOperation>();
            body.SetupGet(instance => instance.Context).Returns(context.Object);
            body.SetupGet(instance => instance.Method).Returns(new List<string>() { Method });
            body.SetupGet(instance => instance.Returns).Returns(new[] { @class.Object });
            return body;
        }

        private IList<EntityQuad> CreateOperationMock(Uri operationUri, Uri classUri)
        {
            IList<EntityQuad> quads = new List<EntityQuad>();
            var graph = new Graph() { BaseUri = (Uri)BaseUrl };
            _tripleStore.Add(graph);
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