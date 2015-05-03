#pragma warning disable 1591
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RomanticWeb;
using RomanticWeb.Entities;
using RomanticWeb.Mapping;
using RomanticWeb.Mapping.Model;
using RomanticWeb.Model;
using RomanticWeb.Vocabularies;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using URSA.CodeGen;
using URSA.Web;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.CodeGen;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Owl;
using IClass = URSA.Web.Http.Description.Hydra.IClass;

namespace Given_instance_of_the.HydraClassGenerator_class
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class when_building_class_code
    {
        private const string Name = "Type";
        private const string Namespace = "Test";
        private const string HttpMethod = "GET";
        private static readonly Uri ClassUri = new Uri(String.Format("urn:net:{0}.{1}", Namespace, Name));
        private static readonly EntityId ClassId = new EntityId(ClassUri);
        private static readonly Uri HydraSupportedOperation = new Uri(EntityConverter.Hydra.AbsoluteUri + "supportedOperation");
        private static readonly Uri HydraIriTemplate = new Uri(EntityConverter.Hydra.AbsoluteUri + "IriTemplate");
        private static readonly Uri HydraTemplatedLink = new Uri(EntityConverter.Hydra.AbsoluteUri + "TemplatedLink");
        private static readonly Uri UrsaDatatypeDefinition = new Uri(DescriptionController<IController>.VocabularyBaseUri + "DatatypeDefinition");
        private static readonly Uri ReadOperationUri = new Uri("http://temp.uri/type/#GET");

        private IList<EntityQuad> _quads; 
        private Mock<IMappingsRepository> _mappingsRepository;
        private Mock<IEntityContext> _context;
        private Mock<IUriParser> _uriParser;
        private Mock<URSA.Web.Http.Description.Rdfs.IProperty> _property;
        private Mock<IClass> _class;
        private Mock<IEntityStore> _store;
        private IClassGenerator _generator;

        [TestMethod]
        public void it_should_generate_entity_class_code()
        {
            var result = _generator.CreateCode(_class.Object);

            result.First().Value.Should().Be(new StreamReader(GetType().Assembly.GetManifestResourceStream("URSA.Http.Description.Tests.Testing.Templates.Type.cs")).ReadToEnd());
        }

        [TestMethod]
        public void it_should_generate_client_class_code()
        {
            var result = _generator.CreateCode(_class.Object);

            result.Last().Value.Should().Be(new StreamReader(GetType().Assembly.GetManifestResourceStream("URSA.Http.Description.Tests.Testing.Templates.TypeClient.cs")).ReadToEnd());
        }

        [TestInitialize]
        public void Setup()
        {
            SetupUriParser();
            SetupContex();
            SetupClass();
            _generator = new HydraClassGenerator(new IUriParser[] { _uriParser.Object });
        }

        [TestCleanup]
        public void Teardown()
        {
            _generator = null;
            _uriParser = null;
            _property = null;
            _class = null;
            _mappingsRepository = null;
            _context = null;
            _store = null;
            _quads = null;
        }

        private void SetupUriParser()
        {
            _uriParser = new Mock<IUriParser>(MockBehavior.Strict);
            _uriParser.Setup(instance => instance.IsApplicable(It.IsAny<Uri>())).Returns(UriParserCompatibility.ExactMatch);
            var @namespace = Namespace;
            _uriParser.Setup(instance => instance.Parse(ClassUri, out @namespace)).Returns<Uri, string>((id, ns) => Name);
            var @systemNamespace = "System";
            _uriParser.Setup(instance => instance.Parse(Xsd.Int, out @systemNamespace)).Returns<Uri, string>((id, ns) => typeof(int).Name);
        }

        private void SetupStore()
        {
            _quads = new List<EntityQuad>();
            _store = new Mock<IEntityStore>(MockBehavior.Strict);
            _store.SetupGet(instance => instance.Quads).Returns(_quads);
            _store.Setup(instance => instance.GetEntityQuads(It.IsAny<EntityId>())).Returns<EntityId>(id => _quads.Where(quad => quad.EntityId == id));
            _context.SetupGet(instance => instance.Store).Returns(_store.Object);
        }

        private void SetupContex()
        {
            _context = new Mock<IEntityContext>(MockBehavior.Strict);
            SetupStore();
            _mappingsRepository = new Mock<IMappingsRepository>(MockBehavior.Strict);
            SetupMapping<IDatatypeDefinition>(DescriptionController<IController>.VocabularyBaseUri);
            SetupMapping<IInverseFunctionalProperty>(new Uri(Owl.BaseUri));
            _context.SetupGet(instance => instance.Mappings).Returns(_mappingsRepository.Object);
        }

        private void SetupClass()
        {
            var rangeType = new Mock<IResource>(MockBehavior.Strict);
            rangeType.SetupGet(instance => instance.Id).Returns(Xsd.Int);
            var range = new Mock<IDatatypeDefinition>(MockBehavior.Strict);
            range.SetupGet(instance => instance.Id).Returns(new EntityId(new Uri("wrapper:xsd:int")));
            range.SetupGet(instance => instance.Datatype).Returns(rangeType.Object);
            range.SetupGet(instance => instance.Context).Returns(_context.Object);
            range.As<ITypedEntity>().SetupGet(instance => instance.Types).Returns(new[] { new EntityId(UrsaDatatypeDefinition) });
            _property = new Mock<URSA.Web.Http.Description.Rdfs.IProperty>(MockBehavior.Strict);
            _property.SetupGet(instance => instance.Id).Returns(new EntityId(new Uri(ClassUri.AbsoluteUri + ".Id")));
            _property.SetupGet(instance => instance.Label).Returns("Id");
            _property.SetupGet(instance => instance.Range).Returns(new[] { range.Object });
            _property.As<ITypedEntity>().SetupGet(instance => instance.Types).Returns(new[] { new EntityId(Owl.InverseFunctionalProperty) });

            var supportedProperty = new Mock<ISupportedProperty>(MockBehavior.Strict);
            supportedProperty.SetupGet(instance => instance.Property).Returns(_property.Object);
            supportedProperty.SetupGet(instance => instance.WriteOnly).Returns(false);
            supportedProperty.SetupGet(instance => instance.ReadOnly).Returns(false);

            var restriction = new Mock<IRestriction>(MockBehavior.Strict);
            restriction.SetupGet(instance => instance.Id).Returns(new EntityId(new Uri("urn:guid:" + Guid.NewGuid())));
            restriction.SetupGet(instance => instance.OnProperty).Returns(_property.Object);
            restriction.SetupGet(instance => instance.MaxCardinality).Returns(1);

            _class = new Mock<IClass>(MockBehavior.Strict);
            _class.SetupGet(instance => instance.Context).Returns(_context.Object);
            _class.SetupGet(instance => instance.Id).Returns(ClassId);
            _class.SetupGet(instance => instance.Label).Returns("Type");
            _class.SetupGet(instance => instance.SupportedProperties).Returns(new[] { supportedProperty.Object });
            _class.SetupGet(instance => instance.SubClassOf).Returns(new[] { restriction.Object });
            _class.As<ITypedEntity>().SetupGet(instance => instance.Types).Returns(new EntityId[0]);
            SetupSupportedOperation();
            SetupTemplatedOperation();
        }

        private void SetupSupportedOperation()
        {
            var readOperationId = new EntityId(ReadOperationUri);
            var readOperation = new Mock<IOperation>(MockBehavior.Strict);
            readOperation.SetupGet(instance => instance.Id).Returns(readOperationId);
            readOperation.SetupGet(instance => instance.Label).Returns("List");
            readOperation.SetupGet(instance => instance.Method).Returns(new string[] { HttpMethod });
            readOperation.SetupGet(instance => instance.Expects).Returns(new IClass[0]);
            readOperation.SetupGet(instance => instance.Returns).Returns(new IClass[0]);
            readOperation.SetupGet(instance => instance.MediaTypes).Returns(new string[0]);
            _context.Setup(instance => instance.Load<IOperation>(readOperationId)).Returns(readOperation.Object);
            _class.SetupGet(instance => instance.SupportedOperations).Returns(new[] { readOperation.Object });
            _quads.Add(new EntityQuad(ClassId, Node.ForUri(ClassUri), Node.ForUri(HydraSupportedOperation), Node.ForUri(ReadOperationUri)));
        }

        private void SetupTemplatedOperation()
        {
            SetupMapping<ITemplatedLink>(EntityConverter.Hydra);
            var baseUri = new Uri("http://temp.uri/type/GETId#");

            var getOperationUri = new Uri(baseUri.AbsoluteUri);
            var getOperationId = new EntityId(getOperationUri);
            var getOperation = new Mock<IOperation>(MockBehavior.Strict);
            getOperation.SetupGet(instance => instance.Id).Returns(getOperationId);
            getOperation.SetupGet(instance => instance.Label).Returns("Get");
            getOperation.SetupGet(instance => instance.Method).Returns(new string[] { HttpMethod });
            getOperation.SetupGet(instance => instance.Expects).Returns(new IClass[0]);
            getOperation.SetupGet(instance => instance.Returns).Returns(new IClass[0]);
            getOperation.SetupGet(instance => instance.MediaTypes).Returns(new string[0]);

            var mapping = new Mock<IIriTemplateMapping>(MockBehavior.Strict);
            mapping.SetupGet(instance => instance.Variable).Returns("id");
            mapping.SetupGet(instance => instance.Property).Returns(_property.Object);
            mapping.SetupGet(instance => instance.Context).Returns(_context.Object);

            var templatedLinkUri = new Uri(baseUri.AbsoluteUri + "withTemplate");
            var templatedLinkId = new EntityId(templatedLinkUri);
            var templatedLink = new Mock<ITemplatedLink>(MockBehavior.Strict);
            templatedLink.SetupGet(instance => instance.Id).Returns(templatedLinkId);
            templatedLink.SetupGet(instance => instance.Operations).Returns(new[] { getOperation.Object });
            templatedLink.As<ITypedEntity>().SetupGet(instance => instance.Types).Returns(new[] { new EntityId(HydraTemplatedLink) });

            var iriTemplateUri = new Uri(baseUri.AbsoluteUri + "template");
            var iriTemplateId = new EntityId(iriTemplateUri);
            var iriTemplate = new Mock<IIriTemplate>(MockBehavior.Strict);
            iriTemplate.SetupGet(instance => instance.Id).Returns(iriTemplateId);
            iriTemplate.SetupGet(instance => instance.Mappings).Returns(new[] { mapping.Object });
            iriTemplate.SetupGet(instance => instance.Template).Returns("/type/id/{?id}");
            iriTemplate.As<ITypedEntity>().SetupGet(instance => instance.Types).Returns(new[] { new EntityId(HydraIriTemplate) });

            _quads.Add(new EntityQuad(ClassId, Node.ForUri(ClassUri), Node.ForUri(templatedLinkUri), Node.ForUri(iriTemplateUri)));

            _context.Setup(instance => instance.Load<IOperation>(getOperationId)).Returns(getOperation.Object);
            _context.Setup(instance => instance.Load<IIriTemplate>(iriTemplateId)).Returns(iriTemplate.Object);
            _context.Setup(instance => instance.Load<IEntity>(templatedLinkId)).Returns(templatedLink.Object);
            _context.Setup(instance => instance.Load<ITemplatedLink>(templatedLinkId)).Returns(templatedLink.Object);
        }

        private void SetupMapping<T>(Uri baseUri) where T : IEntity
        {
            var classMapping = new Mock<IClassMapping>(MockBehavior.Strict);
            classMapping.SetupGet(instance => instance.Uri).Returns(new Uri(baseUri.AbsoluteUri + typeof(T).Name.Substring(1)));
            var mapping = new Mock<IEntityMapping>(MockBehavior.Strict);
            mapping.SetupGet(instance => instance.Classes).Returns(new[] { classMapping.Object });
            _mappingsRepository.Setup(instance => instance.MappingFor<T>()).Returns(mapping.Object);
        }
    }
}