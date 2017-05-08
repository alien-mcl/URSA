using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using RDeF.Entities;
using RDeF.Mapping;
using URSA;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Configuration;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Testing;
using URSA.Web.Http.Tests.Testing;
using URSA.Web.Tests;

namespace Given_instance_of_the
{
    [TestFixture]
    public class ApiEntryPointDescriptionBuilder_class
    {
        private static readonly EntryPointInfo EntryPoint = new EntryPointInfo(UrlParser.Parse("/api"));
        private Mock<IApiDescriptionBuilder> _irrelevantApiDescriptionBuilder;
        private Mock<IApiDescriptionBuilder<TestController>> _apiDescriptionBuilder;
        private Mock<IApiDescriptionBuilderFactory> _apiDescriptionBuilderFactory;
        private Mock<IHttpControllerDescriptionBuilder> _irrelevantControllerDescriptionBuilder;
        private Mock<IHttpControllerDescriptionBuilder<TestController>> _controllerDescriptionBuilder;
        private Mock<IHttpControllerDescriptionBuilder<EntryPointDescriptionController>> _entryPointControllerDescriptionBuilder;
        private Mock<IApiDocumentation> _apiDocumentation;
        private IApiEntryPointDescriptionBuilder _descriptionBuilder;

        [Test]
        public void it_should_create_an_entry_point_documentation()
        {
            _descriptionBuilder.BuildDescription(_apiDocumentation.Object, null);

            _apiDescriptionBuilder.Verify(instance => instance.BuildDescription(_apiDocumentation.Object, null), Times.Once);
        }

        [Test]
        public void it_should_omit_irrelevant_controllers()
        {
            _descriptionBuilder.BuildDescription(_apiDocumentation.Object, null);

            _irrelevantApiDescriptionBuilder.Verify(instance => instance.BuildDescription(_apiDocumentation.Object, null), Times.Never);
        }

        [Test]
        public void it_should_include_ApiDocumentation_supported_class()
        {
            _descriptionBuilder.BuildDescription(_apiDocumentation.Object, null);

            _apiDocumentation.Object.SupportedClasses.Should().HaveCount(1);
        }

        [SetUp]
        public void Setup()
        {
            HttpUrl requestUrl = (HttpUrl)UrlParser.Parse("/test");
            var mappingsRepository = new Mock<IMappingsRepository>(MockBehavior.Strict);
            mappingsRepository.SetupMapping<IApiDocumentation>(EntityConverter.Hydra);
            var classEntity = new Mock<IClass>(MockBehavior.Strict);
            var httpServerConfiguration = new Mock<IHttpServerConfiguration>(MockBehavior.Strict);
            httpServerConfiguration.SetupGet(instance => instance.BaseUri).Returns(new Uri("http://temp.uri/"));
            var context = new Mock<IEntityContext>(MockBehavior.Strict);
            context.SetupGet(instance => instance.Mappings).Returns(mappingsRepository.Object);
            context.Setup(instance => instance.Create<IClass>(It.IsAny<Iri>())).Returns(classEntity.Object);
            _apiDocumentation = new Mock<IApiDocumentation>(MockBehavior.Strict);
            _apiDocumentation.SetupGet(instance => instance.Context).Returns(context.Object);
            _apiDocumentation.SetupGet(instance => instance.SupportedClasses).Returns(new List<IClass>());
            _irrelevantApiDescriptionBuilder = new Mock<IApiDescriptionBuilder>(MockBehavior.Strict);
            _apiDescriptionBuilder = new Mock<IApiDescriptionBuilder<TestController>>(MockBehavior.Strict);
            _apiDescriptionBuilder.Setup(instance => instance.BuildDescription(_apiDocumentation.Object, null));
            _apiDescriptionBuilderFactory = new Mock<IApiDescriptionBuilderFactory>(MockBehavior.Strict);
            _apiDescriptionBuilderFactory.Setup(instance => instance.Create(It.Is<Type>(type => type == typeof(TestController)))).Returns(_apiDescriptionBuilder.Object);
            string callUri;
            var controllerDescription = new ControllerInfo<TestController>(
                EntryPoint,
                requestUrl.InsertSegments(0, ((HttpUrl)EntryPoint.Url).Segments),
                typeof(TestController).GetTypeInfo().GetMethod("Add").ToOperationInfo(requestUrl.InsertSegments(0, ((HttpUrl)EntryPoint.Url).Segments).ToString(), Verb.GET, out callUri));
            _irrelevantControllerDescriptionBuilder = new Mock<IHttpControllerDescriptionBuilder>(MockBehavior.Strict);
            _controllerDescriptionBuilder = new Mock<IHttpControllerDescriptionBuilder<TestController>>(MockBehavior.Strict);
            _controllerDescriptionBuilder.As<IHttpControllerDescriptionBuilder>().Setup(instance => instance.BuildDescriptor()).Returns(controllerDescription);
            var entryPointControllerDescription = new ControllerInfo<EntryPointDescriptionController>(null, UrlParser.Parse("/test"));
            _entryPointControllerDescriptionBuilder = new Mock<IHttpControllerDescriptionBuilder<EntryPointDescriptionController>>(MockBehavior.Strict);
            _entryPointControllerDescriptionBuilder.As<IHttpControllerDescriptionBuilder>().Setup(instance => instance.BuildDescriptor()).Returns(entryPointControllerDescription);
            _descriptionBuilder = new ApiEntryPointDescriptionBuilder(
                httpServerConfiguration.Object,
                _apiDescriptionBuilderFactory.Object,
                new[] { _controllerDescriptionBuilder.Object, _irrelevantControllerDescriptionBuilder.Object, _entryPointControllerDescriptionBuilder.Object });
            _descriptionBuilder.EntryPoint = EntryPoint;
        }

        [TearDown]
        public void Teardown()
        {
            _irrelevantApiDescriptionBuilder = null;
            _apiDocumentation = null;
            _descriptionBuilder = null;
            _irrelevantControllerDescriptionBuilder = null;
            _controllerDescriptionBuilder = null;
            _apiDescriptionBuilder = null;
            _apiDescriptionBuilderFactory = null;
        }
    }
}