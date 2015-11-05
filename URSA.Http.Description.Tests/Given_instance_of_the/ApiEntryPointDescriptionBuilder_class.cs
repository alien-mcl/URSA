using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RomanticWeb;
using RomanticWeb.Entities;
using RomanticWeb.Mapping;
using RomanticWeb.Mapping.Model;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Tests.Testing;
using URSA.Web.Tests;

namespace URSA.Http.Description.Tests.Given_instance_of_the
{
    [TestClass]
    public class ApiEntryPointDescriptionBuilder_class
    {
        private static readonly Uri EntryPoint = new Uri("/api", UriKind.Relative);
        private Mock<IApiDescriptionBuilder> _irrelevantApiDescriptionBuilder;
        private Mock<IApiDescriptionBuilder<TestController>> _apiDescriptionBuilder;
        private Mock<IApiDescriptionBuilderFactory> _apiDescriptionBuilderFactory;
        private Mock<IHttpControllerDescriptionBuilder> _irrelevantControllerDescriptionBuilder;
        private Mock<IHttpControllerDescriptionBuilder<TestController>> _controllerDescriptionBuilder;
        private Mock<IHttpControllerDescriptionBuilder<EntryPointDescriptionController>> _entryPointControllerDescriptionBuilder;
        private Mock<IApiDocumentation> _apiDocumentation;
        private IApiEntryPointDescriptionBuilder _descriptionBuilder;

        [TestMethod]
        public void it_should_create_an_entry_point_documentation()
        {
            _descriptionBuilder.BuildDescription(_apiDocumentation.Object, null);

            _apiDescriptionBuilder.Verify(instance => instance.BuildDescription(_apiDocumentation.Object, null), Times.Once);
        }

        [TestMethod]
        public void it_should_omit_irrelevant_controllers()
        {
            _descriptionBuilder.BuildDescription(_apiDocumentation.Object, null);

            _irrelevantApiDescriptionBuilder.Verify(instance => instance.BuildDescription(_apiDocumentation.Object, null), Times.Never);
        }

        [TestMethod]
        public void it_should_include_ApiDocumentation_supported_class()
        {
            _descriptionBuilder.BuildDescription(_apiDocumentation.Object, null);

            _apiDocumentation.Object.SupportedClasses.Should().HaveCount(1);
        }

        [TestInitialize]
        public void Setup()
        {
            Uri requestUri = new Uri("/test", UriKind.Relative);
            var classMapping = new Mock<IClassMapping>(MockBehavior.Strict);
            classMapping.SetupGet(instance => instance.Uri).Returns(EntityConverter.Hydra.AddFragment("ApiDocumentation"));
            var mapping = new Mock<IEntityMapping>(MockBehavior.Strict);
            mapping.SetupGet(instance => instance.Classes).Returns(new[] { classMapping.Object });
            var mappings = new Mock<IMappingsRepository>(MockBehavior.Strict);
            mappings.Setup(instance => instance.MappingFor(typeof(IApiDocumentation))).Returns(mapping.Object);
            var classEntity = new Mock<IClass>(MockBehavior.Strict);
            var context = new Mock<IEntityContext>(MockBehavior.Strict);
            context.SetupGet(instance => instance.Mappings).Returns(mappings.Object);
            context.Setup(instance => instance.Create<IClass>(It.IsAny<EntityId>())).Returns(classEntity.Object);
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
                requestUri.Combine(EntryPoint),
                typeof(TestController).GetMethod("Add").ToOperationInfo(EntryPoint.ToString() + requestUri, Verb.GET, out callUri));
            _irrelevantControllerDescriptionBuilder = new Mock<IHttpControllerDescriptionBuilder>(MockBehavior.Strict);
            _controllerDescriptionBuilder = new Mock<IHttpControllerDescriptionBuilder<TestController>>(MockBehavior.Strict);
            _controllerDescriptionBuilder.As<IHttpControllerDescriptionBuilder>().Setup(instance => instance.BuildDescriptor()).Returns(controllerDescription);
            var entryPointControllerDescription = new ControllerInfo<EntryPointDescriptionController>(null, new Uri("/test", UriKind.Relative));
            _entryPointControllerDescriptionBuilder = new Mock<IHttpControllerDescriptionBuilder<EntryPointDescriptionController>>(MockBehavior.Strict);
            _entryPointControllerDescriptionBuilder.As<IHttpControllerDescriptionBuilder>().Setup(instance => instance.BuildDescriptor()).Returns(entryPointControllerDescription);
            _descriptionBuilder = new ApiEntryPointDescriptionBuilder(
                _apiDescriptionBuilderFactory.Object,
                new[] { _controllerDescriptionBuilder.Object, _irrelevantControllerDescriptionBuilder.Object, _entryPointControllerDescriptionBuilder.Object });
            _descriptionBuilder.EntryPoint = EntryPoint;
        }

        [TestCleanup]
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