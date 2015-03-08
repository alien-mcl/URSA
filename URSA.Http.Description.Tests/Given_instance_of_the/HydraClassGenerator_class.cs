using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RomanticWeb.Entities;
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
            var readId = new Uri("http://temp.uri/type/");
            const string ReadMethod = "GET";
            var readOperation = new Mock<IOperation>(MockBehavior.Strict);
            readOperation.SetupGet(instance => instance.Id).Returns(new EntityId(readId));
            readOperation.SetupGet(instance => instance.Label).Returns("List");
            readOperation.SetupGet(instance => instance.Method).Returns(new string[] { ReadMethod });
            readOperation.SetupGet(instance => instance.Expects).Returns(new IClass[0]);
            var @namespace = "Test";
            const string Name = "Type";
            var classId = new Uri(String.Format("urn:net:{0}.{1}", @namespace, Name));
            var @class = new Mock<IClass>(MockBehavior.Strict);
            @class.SetupGet(instance => instance.Id).Returns(new EntityId(classId));
            @class.SetupGet(instance => instance.Label).Returns("Type");
            @class.SetupGet(instance => instance.SupportedOperations).Returns(new IOperation[] { readOperation.Object });
            _uriParser.Setup(instance => instance.Parse(classId, out @namespace)).Returns<Uri, string>((id, ns) => Name);

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
               Name,
               ReadMethod,
               readId));
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