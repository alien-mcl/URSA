using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RomanticWeb.Entities;
using RomanticWeb.NamedGraphs;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Description.NamedGraphs;
using URSA.Web.Http.Description.Tests;
using URSA.Web.Http.Tests.Testing;

namespace URSA.Http.Description.Tests.Given_instance_of_the
{
    [TestClass]
    public class OwningResourceNamedGraphSelector_class
    {
        private static readonly Uri BaseUri = new Uri("http://temp.uri/");
        private static readonly Uri EntryPoint = new Uri("/api/person", UriKind.Relative);
        private OperationInfo _operation;
        private INamedGraphSelector _selector;

        [TestMethod]
        public void it_should_match_uri()
        {
            var entityId = new EntityId(EntryPoint.Combine(BaseUri) + "/" + Guid.Empty);

            Uri result = _selector.SelectGraph(entityId, null, null);

            result.Should().Be(entityId.Uri);
        }

        [TestMethod]
        public void it_should_match_uri_with_query_string_parameters()
        {
            var entityId = new EntityId(EntryPoint.Combine(BaseUri) + "/" + Guid.Empty + "?_random=10");

            Uri result = _selector.SelectGraph(entityId, null, null);

            result.Should().Be(entityId.Uri);
        }

        [TestInitialize]
        public void Setup()
        {
            var descriptionBuilder = new Mock<IHttpControllerDescriptionBuilder>(MockBehavior.Strict);
            var controllerInfo = new ControllerInfo<TestController>(
                null,
                EntryPoint,
                typeof(TestController).GetMethod("List").ToOperationInfo(EntryPoint.ToString(), Verb.GET),
                typeof(TestController).GetMethod("Get").ToOperationInfo(EntryPoint.ToString(), Verb.GET),
                typeof(TestController).GetMethod("Create").ToOperationInfo(EntryPoint.ToString(), Verb.GET),
                typeof(TestController).GetMethod("Update").ToOperationInfo(EntryPoint.ToString(), Verb.GET),
                typeof(TestController).GetMethod("Delete").ToOperationInfo(EntryPoint.ToString(), Verb.GET));
            descriptionBuilder.Setup(instance => instance.BuildDescriptor()).Returns(controllerInfo);
            _selector = new OwningResourceNamedGraphSelector(new[] { descriptionBuilder.Object });
        }

        [TestCleanup]
        public void Teardown()
        {
            _selector = null;
        }
    }
}