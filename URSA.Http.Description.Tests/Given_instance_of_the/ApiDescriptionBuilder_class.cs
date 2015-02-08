using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RomanticWeb;
using RomanticWeb.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using URSA.Http.Description.Testing;
using URSA.Web.Description;
using URSA.Web.Http;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Tests;

namespace Given_instance_of_the
{
    [TestClass]
    public class ApiDescriptionBuilder_class
    {
        [TestMethod]
        public void it_should_build_the_api_documentation()
        {
            IApiDocumentation apiDocumentation;
            IHttpDelegateMapper handlerMapper = SetupInfrastucture(out apiDocumentation);
            var apiDescriptionBuilder = new ApiDescriptionBuilder<TestController>(handlerMapper);
            apiDescriptionBuilder.BuildDescription(apiDocumentation);

            apiDocumentation.EntryPoints.Should().HaveCount(5);
            apiDocumentation.EntryPoints.First().Operations.Should().HaveCount(1);
            apiDocumentation.EntryPoints.First().Operations.First().Method.Should().HaveCount(1);
            apiDocumentation.EntryPoints.First().Operations.First().Method.First().Should().Be("GET");
        }

        private IHttpDelegateMapper SetupInfrastucture(out IApiDocumentation apiDocumentationInstance)
        {
            IDictionary<string, Verb> verbs = new Dictionary<string, Verb>()
            {
                { "Get", Verb.GET },
                { "Create", Verb.POST },
                { "Update", Verb.PUT },
                { "Delete", Verb.DELETE }
            };
            Uri baseUri = new Uri("http://temp.org/");
            IEnumerable<ArgumentInfo> parameterMapping = new List<ArgumentInfo>();
            Mock<IHttpDelegateMapper> handlerMapper = new Mock<IHttpDelegateMapper>();
            handlerMapper.Setup(instance => instance.GetMethodVerb(It.IsAny<MethodInfo>()))
                .Returns<MethodInfo>(method => verbs[method.Name]);
            handlerMapper.Setup(instance => instance.GetMethodUriTemplate(It.IsAny<MethodInfo>(), out parameterMapping))
                .Returns<MethodInfo, IEnumerable<ArgumentInfo>>((method, parameters) => 
                    (method.GetParameters().Length > 0 ? new Uri(baseUri, "api/" + method.Name.ToLower()).ToString() : null));
            Mock<IBaseUriSelectionPolicy> baseUriSelector = new Mock<IBaseUriSelectionPolicy>();
            baseUriSelector.Setup(instance => instance.SelectBaseUri(It.IsAny<EntityId>())).Returns(baseUri);
            Mock<IEntityContext> context = new Mock<IEntityContext>();
            context.SetupGet(instance => instance.BaseUriSelector).Returns(baseUriSelector.Object);
            context.Setup(instance => instance.Create<IEntity>(It.IsAny<EntityId>()))
                .Returns<EntityId>(id => MockHelpers.MockEntity<IEntity>(context.Object, id).Object);
            context.Setup(instance => instance.Create<IResource>(It.IsAny<EntityId>()))
                .Returns<EntityId>(id => MockHelpers.MockEntity<IResource>(context.Object, id).Object);
            context.Setup(instance => instance.Create<IProperty>(It.IsAny<EntityId>()))
                .Returns<EntityId>(id => MockHelpers.MockEntity<IProperty>(context.Object, id).Object);
            context.Setup(instance => instance.Create<ISupportedProperty>(It.IsAny<EntityId>()))
                .Returns<EntityId>(id => MockHelpers.MockEntity<ISupportedProperty>(context.Object, id).Object);
            context.Setup(instance => instance.Create<IClass>(It.IsAny<EntityId>()))
                .Returns<EntityId>(id => MockHelpers.MockEntity<IClass>(context.Object, id).Object);
            context.Setup(instance => instance.Create<IOperation>(It.IsAny<EntityId>()))
                .Returns<EntityId>(id => MockHelpers.MockEntity<IOperation>(context.Object, id).Object);
            var apiDocumentation = MockHelpers.MockEntity<IApiDocumentation>(context.Object, new EntityId(new Uri(baseUri, "api")));
            apiDocumentationInstance = apiDocumentation.Object;
            return handlerMapper.Object;
        }
    }
}