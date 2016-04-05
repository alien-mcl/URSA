using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using URSA.Security;
using URSA.Web;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the.DelegateMapper_class
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class when_mapping_a_request
    {
        private DelegateMapper _mapper;

        [TestMethod]
        public void it_should_map_Add_method_correctly()
        {
            var mapping = _mapper.MapRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test/add?operandA=1&operandB=2"), new MemoryStream(), new BasicClaimBasedIdentity()));

            mapping.Should().NotBeNull();
            mapping.Target.Should().NotBeNull();
            mapping.Target.Should().BeOfType<TestController>();
            mapping.Operation.UnderlyingMethod.Should().BeSameAs(typeof(TestController).GetMethod("Add"));
        }

        [TestMethod]
        public void it_should_map_Add_method_correctly_with_reversed_query_string_parameters()
        {
            var mapping = _mapper.MapRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test/add?operandB=2&operandA=1"), new MemoryStream(), new BasicClaimBasedIdentity()));

            mapping.Should().NotBeNull();
            mapping.Target.Should().NotBeNull();
            mapping.Target.Should().BeOfType<TestController>();
            mapping.Operation.UnderlyingMethod.Should().BeSameAs(typeof(TestController).GetMethod("Add"));
        }

        [TestMethod]
        public void it_should_map_Add_method_correctly_with_additional_unneeded_parameters()
        {
            var mapping = _mapper.MapRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test/add?random=1&operandB=2&_=whatever&operandA=1"), new MemoryStream(), new BasicClaimBasedIdentity()));

            mapping.Should().NotBeNull();
            mapping.Target.Should().NotBeNull();
            mapping.Target.Should().BeOfType<TestController>();
            mapping.Operation.UnderlyingMethod.Should().BeSameAs(typeof(TestController).GetMethod("Add"));
        }

        [TestInitialize]
        public void Setup()
        {
            var method = typeof(TestController).GetMethod("Add");
            var baseUri = new Uri("/api/test/", UriKind.Relative);
            var operationUri = new Uri("/add", UriKind.Relative).Combine(baseUri);
            var operation = new OperationInfo<Verb>(
                method,
                operationUri,
                operationUri + "?operandA={?operandA}&operandB={?operandB}",
                new Regex(operationUri.ToString(), RegexOptions.IgnoreCase),
                Verb.GET,
                new ArgumentInfo(method.GetParameters()[0], FromQueryStringAttribute.For(method.GetParameters()[0]), "&operandA={?operandA}", "operandA"),
                new ArgumentInfo(method.GetParameters()[1], FromQueryStringAttribute.For(method.GetParameters()[1]), "&operandB={?operandB}", "operandB"));
            var description = new ControllerInfo<TestController>(null, new Uri("/api/test/", UriKind.Relative), operation);
            Mock<IHttpControllerDescriptionBuilder<TestController>> builder = new Mock<IHttpControllerDescriptionBuilder<TestController>>();
            builder.As<IControllerDescriptionBuilder>().Setup(instance => instance.BuildDescriptor()).Returns(description);
            Mock<IControllerActivator> activator = new Mock<IControllerActivator>();
            activator.Setup(instance => instance.CreateInstance(It.IsAny<Type>(), It.IsAny<IDictionary<string, object>>())).Returns(new TestController());
            _mapper = new DelegateMapper(new IHttpControllerDescriptionBuilder[] { builder.Object }, activator.Object);
        }

        [TestCleanup]
        public void Teardown()
        {
            _mapper = null;
        }
    }
}