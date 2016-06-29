using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RomanticWeb.Entities;
using RomanticWeb.NamedGraphs;
using URSA;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Description.NamedGraphs;
using URSA.Web.Http.Description.Tests;
using URSA.Web.Http.Tests.Testing;

namespace Given_instance_of_the
{
    //// TODO: Consider removing custom named graph selectors

    [TestClass]
    public class OwningResourceNamedGraphSelector_class
    {
        private static readonly HttpUrl BaseUri = (HttpUrl)UrlParser.Parse("http://temp.uri/");
        private static readonly HttpUrl EntryPoint = (HttpUrl)UrlParser.Parse("/api/person");
        private INamedGraphSelector _selector;

        [TestMethod]
        public void it_should_match_uri()
        {
            var entityId = new EntityId((Uri)(BaseUri + EntryPoint).AddSegment(Guid.Empty.ToString()));

            Uri result = _selector.SelectGraph(entityId, null, null);

            result.Should().Be(entityId.Uri);
        }

        [TestMethod]
        public void it_should_match_uri_with_query_string_parameters()
        {
            var entityId = new EntityId((Uri)(BaseUri + EntryPoint).AddSegment(Guid.Empty.ToString()).WithParameter("_random", "10"));

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