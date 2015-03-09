using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RomanticWeb;
using RomanticWeb.Entities;
using RomanticWeb.Linq.Model;
using RomanticWeb.Model;
using RomanticWeb.Ontologies;
using RomanticWeb.Vocabularies;
using URSA.CodeGen;
using URSA.Web;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.CodeGen;
using URSA.Web.Http.Description.Hydra;

namespace Given_instance_of_the
{
    [TestClass]
    public class HydraClassGenerator_class
    {
        private const string Name = "Class";
        private const string Namespace = "Namespace";

        private Mock<IUriParser> _uriParser;
        private Mock<IResource> _resource;
        private IClassGenerator _generator;

        [TestMethod]
        public void it_should_extract_namespace_from_uri()
        {
            var @namespace = Namespace;
            var result = _generator.CreateNamespace(_resource.Object);

            result.Should().Be(Namespace);
            _uriParser.Verify(instance => instance.IsApplicable(It.IsAny<Uri>()), Times.Once);
            _uriParser.Verify(instance => instance.Parse(It.IsAny<Uri>(), out @namespace), Times.Once);
        }

        [TestMethod]
        public void it_should_extract_name_from_uri()
        {
            var @namespace = Namespace;
            var result = _generator.CreateName(_resource.Object);

            result.Should().Be(Name);
            _uriParser.Verify(instance => instance.IsApplicable(It.IsAny<Uri>()), Times.Once);
            _uriParser.Verify(instance => instance.Parse(It.IsAny<Uri>(), out @namespace), Times.Once);
        }

        [TestMethod]
        public void it_should_generate_class_code()
        {
            Uri hydraSupportedOperation = new Uri(HydraUriParser.HyDrA + "supportedOperation");
            Uri hydraIriTemplate = new Uri(HydraUriParser.HyDrA + "IriTemplate");
            const string HttpMethod = "GET";
            var readOperationUri = new Uri("http://temp.uri/type/");
            var readOperationId = new EntityId(readOperationUri);
            var readOperation = new Mock<IOperation>(MockBehavior.Strict);
            readOperation.SetupGet(instance => instance.Id).Returns(readOperationId);
            readOperation.SetupGet(instance => instance.Label).Returns("List");
            readOperation.SetupGet(instance => instance.Method).Returns(new string[] { HttpMethod });
            readOperation.SetupGet(instance => instance.Expects).Returns(new IClass[0]);
            var getOperationUri = new Uri("http://temp.uri/type/#withId");
            var getOperationId = new EntityId(getOperationUri);
            var getOperation = new Mock<IOperation>(MockBehavior.Strict);
            getOperation.SetupGet(instance => instance.Id).Returns(getOperationId);
            getOperation.SetupGet(instance => instance.Label).Returns("Get");
            getOperation.SetupGet(instance => instance.Method).Returns(new string[] { HttpMethod });
            getOperation.SetupGet(instance => instance.Expects).Returns(new IClass[0]);
            var rangeType = new Mock<URSA.Web.Http.Description.Rdfs.IResource>(MockBehavior.Strict);
            rangeType.SetupGet(instance => instance.Id).Returns(Xsd.Int);
            var range = new List<URSA.Web.Http.Description.Rdfs.IResource>();
            range.Add(rangeType.Object);
            var property = new Mock<URSA.Web.Http.Description.Rdfs.IProperty>(MockBehavior.Strict);
            property.SetupGet(instance => instance.Range).Returns(range);
            var mapping = new Mock<IIriTemplateMapping>(MockBehavior.Strict);
            mapping.SetupGet(instance => instance.Variable).Returns("id");
            mapping.SetupGet(instance => instance.Property).Returns(property.Object);
            var mappings = new List<IIriTemplateMapping>() { mapping.Object };
            var iriTemplateUri = new Uri("http://temp.uri/type/withId");
            var iriTemplateId = new EntityId(iriTemplateUri);
            var iriTemplate = new Mock<IIriTemplate>(MockBehavior.Strict);
            iriTemplate.SetupGet(instance => instance.Id).Returns(iriTemplateId);
            iriTemplate.SetupGet(instance => instance.Template).Returns("http://temp.uri/type/{?id}");
            iriTemplate.SetupGet(instance => instance.Mappings).Returns(mappings);
            var @namespace = "Test";
            const string ClassName = "Type";
            var classUri = new Uri(String.Format("urn:net:{0}.{1}", @namespace, ClassName));
            var classId = new EntityId(classUri);
            var quads = new List<EntityQuad>()
            {
                new EntityQuad(classId, Node.ForUri(classUri), Node.ForUri(hydraSupportedOperation), Node.ForUri(readOperationUri)),
                new EntityQuad(classId, Node.ForUri(classUri), Node.ForUri(iriTemplateUri), Node.ForUri(getOperationUri)),
                new EntityQuad(iriTemplateId, Node.ForUri(iriTemplateUri), Node.ForUri(Rdf.type), Node.ForUri(hydraIriTemplate))
            };
            var store = new Mock<IEntityStore>(MockBehavior.Strict);
            store.SetupGet(instance => instance.Quads).Returns(quads);
            store.Setup(instance => instance.GetEntityQuads(It.IsAny<EntityId>())).Returns<EntityId>(id => quads.Where(quad => quad.EntityId == id));
            var context = new Mock<IEntityContext>(MockBehavior.Strict);
            context.SetupGet(instance => instance.Store).Returns(store.Object);
            context.Setup(instance => instance.Load<IOperation>(readOperationId)).Returns(readOperation.Object);
            context.Setup(instance => instance.Load<IOperation>(getOperationId)).Returns(getOperation.Object);
            context.Setup(instance => instance.Load<IIriTemplate>(iriTemplateId)).Returns(iriTemplate.Object);
            var @class = new Mock<IClass>(MockBehavior.Strict);
            @class.SetupGet(instance => instance.Context).Returns(context.Object);
            @class.SetupGet(instance => instance.Id).Returns(classId);
            @class.SetupGet(instance => instance.Label).Returns("Type");
            @class.SetupGet(instance => instance.SupportedOperations).Returns(new IOperation[] { readOperation.Object });
            _uriParser.Setup(instance => instance.Parse(classUri, out @namespace)).Returns<Uri, string>((id, ns) => ClassName);
            var @systemNamespace = "System";
            _uriParser.Setup(instance => instance.Parse(Xsd.Int, out @systemNamespace)).Returns<Uri, string>((id, ns) => typeof(int).Name);

            var result = _generator.CreateCode(@class.Object);

            result.Should().Be(String.Format(
                @"using System;
using URSA.Web.Http;

namespace {0}
{{
    public class {1} : Client
    {{
        public Type(Uri baseUri) : base(baseUri)
        {{
        }}

        public void List()
        {{
            Call(Verb.{2}, new Uri(""{3}""));
        }}

    }}
}}",
               @namespace,
               ClassName,
               HttpMethod,
               readOperationUri));
        }

        [TestInitialize]
        public void Setup()
        {
            var uri = new Uri("some:uri");
            var @namespace = Namespace;
            _uriParser = new Mock<IUriParser>(MockBehavior.Strict);
            _uriParser.Setup(instance => instance.IsApplicable(It.IsAny<Uri>())).Returns(UriParserCompatibility.ExactMatch);
            _uriParser.Setup(instance => instance.Parse(It.IsAny<Uri>(), out @namespace)).Returns<Uri, string>((id, ns) => Name);
            _resource = new Mock<IResource>(MockBehavior.Strict);
            _resource.SetupGet(instance => instance.Id).Returns(uri);
            _generator = new HydraClassGenerator(new IUriParser[] { _uriParser.Object });
        }

        [TestCleanup]
        public void Teardown()
        {
            _generator = null;
            _uriParser = null;
            _resource = null;
        }
    }
}