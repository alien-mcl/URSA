using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RomanticWeb;
using RomanticWeb.DotNetRDF;
using RomanticWeb.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Testing;
using URSA.Web.Http.Description.Tests;
using URSA.Web.Mapping;
using VDS.RDF;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ApiDescriptionBuilder_class
    {
        [TestMethod]
        public void it_should_build_the_api_documentation()
        {
            IApiDocumentation apiDocumentation;
            var handlerMapper = SetupInfrastucture(out apiDocumentation);
            var apiDescriptionBuilder = new ApiDescriptionBuilder<TestController>(handlerMapper);
            apiDescriptionBuilder.BuildDescription(apiDocumentation);

            apiDocumentation.EntryPoints.Should().HaveCount(0);
            apiDocumentation.SupportedClasses.Should().HaveCount(1);
            apiDocumentation.SupportedClasses.First().SupportedOperations.Should().HaveCount(1);
            apiDocumentation.SupportedClasses.First().SupportedOperations.First().Method.Should().HaveCount(1);
            apiDocumentation.SupportedClasses.First().SupportedOperations.First().Method.First().Should().Be("POST");
        }

        private IHttpControllerDescriptionBuilder<TestController> SetupInfrastucture(out IApiDocumentation apiDocumentationInstance)
        {
            IDictionary<string, Verb> verbs = new Dictionary<string, Verb>()
            {
                { "Get", Verb.GET },
                { "Create", Verb.POST },
                { "Update", Verb.PUT },
                { "Delete", Verb.DELETE }
            };
            Uri baseUri = new Uri("http://temp.org/");
            var operations = typeof(TestController)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method => method.DeclaringType != typeof(object))
                .Except(typeof(TestController).GetProperties(BindingFlags.Public | BindingFlags.Instance).SelectMany(property => new MethodInfo[] { property.GetGetMethod(), property.GetSetMethod() }))
                .Select(method => CreateOperation(method, verbs[method.Name]));

            ControllerInfo<TestController> controllerInfo = new ControllerInfo<TestController>(new Uri("/", UriKind.Relative), operations.ToArray());

            Mock<IHttpControllerDescriptionBuilder<TestController>> descriptionBuilder = new Mock<IHttpControllerDescriptionBuilder<TestController>>();
            descriptionBuilder.Setup(instance => instance.BuildDescriptor()).Returns(controllerInfo);
            descriptionBuilder.Setup(instance => instance.GetMethodVerb(It.IsAny<MethodInfo>())).Returns<MethodInfo>(method => verbs[method.Name]);
            foreach (var operation in operations)
            {
                IEnumerable<ArgumentInfo> parameterMapping = operation.Arguments;
                descriptionBuilder.Setup(instance => instance.GetOperationUriTemplate(operation.UnderlyingMethod, out parameterMapping))
                    .Returns<MethodInfo, IEnumerable<ArgumentInfo>>((method, parameters) => operation.UriTemplate);
            }

            Mock<IBaseUriSelectionPolicy> baseUriSelector = new Mock<IBaseUriSelectionPolicy>();
            baseUriSelector.Setup(instance => instance.SelectBaseUri(It.IsAny<EntityId>())).Returns(baseUri);

            var ontology = new Mock<RomanticWeb.Ontologies.IOntologyProvider>();
            ontology.Setup(instance => instance.ResolveUri("hydra", It.IsAny<string>())).Returns<string, string>((prefix, term) => new Uri("http://www.w3.org/ns/hydra/core#" + term));
            var factory = EntityContextFactory.FromConfiguration("http").WithDefaultOntologies().WithDotNetRDF(new TripleStore()).WithBaseUri(policy => policy.Default.Is(baseUri));
            var apiDocumentation = MockHelpers.MockEntity<IApiDocumentation>(factory.CreateContext(), new EntityId(new Uri(baseUri, "api")));
            apiDocumentationInstance = apiDocumentation.Object;
            return descriptionBuilder.Object;
        }

        private OperationInfo<Verb> CreateOperation(MethodInfo method, Verb verb)
        {
            string queryString = String.Empty;
            string uriTemplate = null;
            IList<ArgumentInfo> arguments = new List<ArgumentInfo>();
            foreach (var parameter in method.GetParameters())
            {
                if (!parameter.IsOut)
                {
                    string parameterTemplate = null;
                    ParameterSourceAttribute source = null;
                    if (parameter.ParameterType == typeof(Guid))
                    {
                        source = FromUriAttribute.For(parameter);
                        uriTemplate += (parameterTemplate = "/" + parameter.Name + "/{?value}");
                    }
                    else if (parameter.ParameterType.IsValueType)
                    {
                        source = FromQueryStringAttribute.For(parameter);
                        queryString += (parameterTemplate = "&" + parameter.Name + "={?value}");
                    }
                    else if (!parameter.ParameterType.IsValueType)
                    {
                        source = FromBodyAttribute.For(parameter);
                    }

                    arguments.Add(new ArgumentInfo(parameter, source, parameterTemplate, (parameterTemplate != null ? parameter.Name : null)));
                }
            }

            if (queryString.Length > 0)
            {
                uriTemplate += "?" + queryString.Substring(1);
            }

            return new OperationInfo<Verb>(method, new Uri("/", UriKind.Relative), uriTemplate, new Regex(".*"), verb, arguments.ToArray());
        }
    }
}