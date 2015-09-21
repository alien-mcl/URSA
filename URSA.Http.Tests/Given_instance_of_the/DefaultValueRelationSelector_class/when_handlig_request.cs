using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the.DefaultValueRelationSelector_class
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class when_handlig_request
    {
        private DefaultValueRelationSelector _selector;

        [TestMethod]
        public void it_should_provide_a_parameter_source_for_Guid()
        {
            var result = _selector.ProvideDefault(typeof(TestController).GetMethod("GetByGuid").GetParameters()[0], Verb.GET);

            result.Should().BeOfType<FromUriAttribute>();
        }

        [TestMethod]
        public void it_should_provide_a_parameter_source_for_int()
        {
            var result = _selector.ProvideDefault(typeof(TestController).GetMethod("Add").GetParameters()[0], Verb.GET);

            result.Should().BeOfType<FromQueryStringAttribute>();
        }

        [TestMethod]
        public void it_should_provide_a_parameter_source_for_Id()
        {
            var result = _selector.ProvideDefault(typeof(TestController).GetMethod("GetBySomeId").GetParameters()[0], Verb.GET);

            result.Should().BeOfType<FromUriAttribute>();
        }

        [TestMethod]
        public void it_should_provide_a_parameter_source_for_object()
        {
            var result = _selector.ProvideDefault(typeof(TestController).GetMethod("GetSomething").GetParameters()[0], Verb.GET);

            result.Should().BeOfType<FromBodyAttribute>();
        }

        [TestInitialize]
        public void Setup()
        {
            _selector = new DefaultValueRelationSelector();
        }

        [TestCleanup]
        public void Teardown()
        {
            _selector = null;
        }
    }
}