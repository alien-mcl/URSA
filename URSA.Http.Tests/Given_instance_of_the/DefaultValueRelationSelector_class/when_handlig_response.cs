using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Description.Http;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the.DefaultValueRelationSelector_class
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class when_handlig_response
    {
        private DefaultValueRelationSelector _selector;

        [TestMethod]
        public void it_should_provide_a_parameter_target_for_Guid()
        {
            var result = _selector.ProvideDefault(typeof(TestController).GetTypeInfo().GetMethod("GetByGuid").GetParameters()[1]);

            result.Should().BeOfType<ToHeaderAttribute>();
        }

        [TestMethod]
        public void it_should_provide_a_parameter_target_for_int()
        {
            var result = _selector.ProvideDefault(typeof(TestController).GetTypeInfo().GetMethod("Add").ReturnParameter);

            result.Should().BeOfType<ToBodyAttribute>();
        }

        [TestMethod]
        public void it_should_provide_a_parameter_target_for_Id()
        {
            var result = _selector.ProvideDefault(typeof(TestController).GetTypeInfo().GetMethod("GetBySomeId").GetParameters()[1]);

            result.Should().BeOfType<ToHeaderAttribute>();
        }

        [TestMethod]
        public void it_should_provide_a_parameter_target_for_object()
        {
            var result = _selector.ProvideDefault(typeof(TestController).GetTypeInfo().GetMethod("GetSomething").GetParameters()[0]);

            result.Should().BeOfType<ToBodyAttribute>();
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