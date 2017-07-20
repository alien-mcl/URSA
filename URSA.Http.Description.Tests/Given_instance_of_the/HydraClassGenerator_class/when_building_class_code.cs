#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using RDeF.Entities;
using RDeF.Mapping;
using RDeF.Vocabularies;
using RollerCaster;
using URSA.CodeGen;
using URSA.Http.Description.Tests.FluentAssertions;
using URSA.Web;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.CodeGen;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Owl;
using URSA.Web.Http.Description.Testing;
using IClass = URSA.Web.Http.Description.Hydra.IClass;

namespace Given_instance_of_the.HydraClassGenerator_class
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class when_building_class_code
    {
        private const string Name = "Type";
        private const string Namespace = "Test";
        private const string HttpMethod = "GET";
        private static readonly Uri ClassUri = new Uri(String.Format("javascript:{0}.{1}", Namespace, Name));
        private static readonly Iri ClassId = new Iri(ClassUri);
        private static readonly Uri HydraSupportedOperation = new Uri(EntityConverter.Hydra.AbsoluteUri + "supportedOperation");
        private static readonly Uri HydraIriTemplate = new Uri(EntityConverter.Hydra.AbsoluteUri + "IriTemplate");
        private static readonly Uri HydraTemplatedLink = new Uri(EntityConverter.Hydra.AbsoluteUri + "TemplatedLink");
        private static readonly Uri UrsaDatatypeDefinition = new Uri(DescriptionController<IController>.VocabularyBaseUri + "DatatypeDefinition");
        private static readonly Uri ReadOperationUri = new Uri("http://temp.uri/type/#GET");

        private IList<Statement> _statements;
        private Mock<IMappingsRepository> _mappingsRepository;
        private Mock<IEntityContext> _context;
        private Mock<IUriParser> _uriParser;
        private Mock<URSA.Web.Http.Description.Rdfs.IProperty> _property;
        private Mock<IClass> _class;
        private Mock<ISerializableEntitySource> _entitySource;
        private IClassGenerator _generator;

        [Test]
        public void it_should_generate_entity_class_code()
        {
            var result = _generator.CreateCode(_class.Object);

            result.First().Value.Should().BeEquivalentToStream("URSA.Http.Description.Tests.Testing.Templates.Type.cs_");
        }

        [Test]
        public void it_should_generate_client_class_code()
        {
            var result = _generator.CreateCode(_class.Object);

            result.Last().Value.Should().BeEquivalentToStream("URSA.Http.Description.Tests.Testing.Templates.TypeClient.cs_");
        }

        [SetUp]
        public void Setup()
        {
            SetupUriParser();
            var apiDocumentation = SetupContex();
            SetupClass();
            apiDocumentation.SetupGet(instance => instance.SupportedClasses).Returns(new[] { _class.Object });
            _generator = new HydraClassGenerator(new[] { _uriParser.Object });
        }

        [TearDown]
        public void Teardown()
        {
            _generator = null;
            _uriParser = null;
            _property = null;
            _class = null;
            _mappingsRepository = null;
            _context = null;
            _entitySource = null;
            _statements = null;
        }

        private void SetupUriParser()
        {
            _uriParser = new Mock<IUriParser>(MockBehavior.Strict);
            _uriParser.Setup(instance => instance.IsApplicable(It.IsAny<Uri>())).Returns(UriParserCompatibility.ExactMatch);
            var @namespace = Namespace;
            _uriParser.Setup(instance => instance.Parse(ClassUri, out @namespace)).Returns<Uri, string>((id, ns) => Name);
            var @systemNamespace = "System";
            _uriParser.Setup(instance => instance.Parse(xsd.@int, out @systemNamespace)).Returns<Uri, string>((id, ns) => typeof(int).Name);
        }

        private void SetupStore()
        {
            _statements = new List<Statement>();
            _entitySource = new Mock<ISerializableEntitySource>(MockBehavior.Strict);
            _entitySource.SetupGet(instance => instance.Statements).Returns(_statements);
            _entitySource.Setup(instance => instance.Load(It.IsAny<Iri>())).Returns<Iri>(id => _statements.Where(statement => statement.Subject == id));
            _context.SetupGet(instance => instance.EntitySource).Returns(_entitySource.Object);
        }

        private Mock<IApiDocumentation> SetupContex()
        {
            _context = new Mock<IEntityContext>(MockBehavior.Strict);
            SetupStore();
            _mappingsRepository = new Mock<IMappingsRepository>(MockBehavior.Strict);
            _mappingsRepository.SetupMapping<IInverseFunctionalProperty>(owl.Namespace);
            _mappingsRepository.SetupMapping<ICreateResourceOperation>(new Iri(EntityConverter.Hydra.AbsoluteUri + "CreateResourceOperation"));
            _mappingsRepository.SetupMapping<ICollection>(new Iri(EntityConverter.Hydra.AbsoluteUri + "Collection"));
            var apiDocumentation = new Mock<IApiDocumentation>(MockBehavior.Strict);
            _context.SetupGet(instance => instance.Mappings).Returns(_mappingsRepository.Object);
            _context.Setup(instance => instance.AsQueryable<IApiDocumentation>()).Returns(new[] { apiDocumentation.Object }.AsQueryable());
            return apiDocumentation;
        }

        private void SetupClass()
        {
            var entityMock = new Mock<MulticastObject>(MockBehavior.Strict);
            var range = entityMock.As<IClass>();
            range.SetupGet(instance => instance.Iri).Returns(xsd.@int);
            range.SetupGet(instance => instance.Context).Returns(_context.Object);
            range.SetupGet(instance => instance.SubClassOf).Returns(new URSA.Web.Http.Description.Rdfs.IClass[0]);
            entityMock.SetupGet(instance => instance.CastedTypes).Returns(new[] { typeof(IClass) });
            entityMock = new Mock<MulticastObject>(MockBehavior.Strict);
            _property = entityMock.As<URSA.Web.Http.Description.Rdfs.IProperty>();
            _property.SetupGet(instance => instance.Iri).Returns(new Iri(new Uri(ClassUri.AbsoluteUri + ".Id")));
            _property.SetupGet(instance => instance.Label).Returns("Id");
            _property.SetupGet(instance => instance.Range).Returns(new[] { range.Object });
            entityMock.SetupGet(instance => instance.CastedTypes).Returns(new[] { typeof(URSA.Web.Http.Description.Rdfs.IProperty) });

            var supportedProperty = new Mock<ISupportedProperty>(MockBehavior.Strict);
            supportedProperty.SetupGet(instance => instance.Property).Returns(_property.Object);
            supportedProperty.SetupGet(instance => instance.Writeable).Returns(true);
            supportedProperty.SetupGet(instance => instance.Readable).Returns(true);

            var restriction = new Mock<IRestriction>(MockBehavior.Strict);
            restriction.SetupGet(instance => instance.Iri).Returns(new Iri(new Uri("urn:guid:" + Guid.NewGuid())));
            restriction.SetupGet(instance => instance.OnProperty).Returns(_property.Object);
            restriction.SetupGet(instance => instance.MaxCardinality).Returns(1);
            restriction.SetupGet(instance => instance.SubClassOf).Returns(new URSA.Web.Http.Description.Rdfs.IClass[0]);

            entityMock = new Mock<MulticastObject>(MockBehavior.Strict);
            _class = entityMock.As<IClass>();
            _class.SetupGet(instance => instance.Context).Returns(_context.Object);
            _class.SetupGet(instance => instance.Iri).Returns(ClassId);
            _class.SetupGet(instance => instance.Label).Returns("Type");
            _class.SetupGet(instance => instance.SupportedProperties).Returns(new[] { supportedProperty.Object });
            _class.SetupGet(instance => instance.SubClassOf).Returns(new[] { restriction.Object });
            entityMock.SetupGet(instance => instance.CastedTypes).Returns(Type.EmptyTypes);
            SetupSupportedOperation();
            SetupTemplatedOperation();
        }

        private void SetupSupportedOperation()
        {
            var readOperationId = new Iri(ReadOperationUri);
            var entityMock = new Mock<MulticastObject>(MockBehavior.Strict);
            var readOperation = entityMock.As<IOperation>();
            entityMock.SetupGet(instance => instance.CastedTypes).Returns(Type.EmptyTypes);
            readOperation.SetupGet(instance => instance.Iri).Returns(readOperationId);
            readOperation.SetupGet(instance => instance.Label).Returns("List");
            readOperation.SetupGet(instance => instance.Method).Returns(new string[] { HttpMethod });
            readOperation.SetupGet(instance => instance.Expects).Returns(new IClass[0]);
            readOperation.SetupGet(instance => instance.Returns).Returns(new IClass[0]);
            readOperation.SetupGet(instance => instance.MediaTypes).Returns(new string[0]);
            _context.Setup(instance => instance.Load<IOperation>(readOperationId)).Returns(readOperation.Object);
            _class.SetupGet(instance => instance.SupportedOperations).Returns(new[] { readOperation.Object });
            _statements.Add(new Statement(ClassUri, HydraSupportedOperation, ReadOperationUri));
        }

        private void SetupTemplatedOperation()
        {
            _mappingsRepository.SetupMapping<ITemplatedLink>(EntityConverter.Hydra);
            var baseUri = new Uri("http://temp.uri/type/GETId#");

            var getOperationUri = new Uri(baseUri.AbsoluteUri);
            var getOperationId = new Iri(getOperationUri);
            var entityMock = new Mock<MulticastObject>(MockBehavior.Strict);
            var getOperation = entityMock.As<IOperation>();
            entityMock.SetupGet(instance => instance.CastedTypes).Returns(Type.EmptyTypes);
            getOperation.SetupGet(instance => instance.Iri).Returns(getOperationId);
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
            var templatedLinkId = new Iri(templatedLinkUri);
            entityMock = new Mock<MulticastObject>(MockBehavior.Strict);
            var templatedLink = entityMock.As<ITemplatedLink>();
            templatedLink.SetupGet(instance => instance.Context).Returns(_context.Object);
            templatedLink.SetupGet(instance => instance.Iri).Returns(templatedLinkId);
            templatedLink.SetupGet(instance => instance.SupportedOperations).Returns(new[] { getOperation.Object });
            entityMock.SetupGet(instance => instance.CastedTypes).Returns(new[] { typeof(ITemplatedLink) });

            var iriTemplateUri = new Uri(baseUri.AbsoluteUri + "template");
            var iriTemplateId = new Iri(iriTemplateUri);
            entityMock = new Mock<MulticastObject>(MockBehavior.Strict);
            var iriTemplate = entityMock.As<IIriTemplate>();
            iriTemplate.SetupGet(instance => instance.Iri).Returns(iriTemplateId);
            iriTemplate.SetupGet(instance => instance.Mappings).Returns(new[] { mapping.Object });
            iriTemplate.SetupGet(instance => instance.Template).Returns("/type/id/{?id}");
            entityMock.SetupGet(instance => instance.CastedTypes).Returns(new[] { typeof(IIriTemplate) });

            _statements.Add(new Statement(ClassUri, templatedLinkUri, iriTemplateUri));

            _context.Setup(instance => instance.Load<IOperation>(getOperationId)).Returns(getOperation.Object);
            _context.Setup(instance => instance.Load<IIriTemplate>(iriTemplateId)).Returns(iriTemplate.Object);
            _context.Setup(instance => instance.Load<IEntity>(templatedLinkId)).Returns(templatedLink.Object);
            _context.Setup(instance => instance.Load<ITemplatedLink>(templatedLinkId)).Returns(templatedLink.Object);
        }
    }
}