using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using URSA.Web.Description.Http;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the.DefaultValueRelationSelector_class
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class when_handlig_response
    {
        private DefaultValueRelationSelector _selector;

        [Test]
        public void it_should_provide_a_parameter_target_for_Guid()
        {
            var result = _selector.ProvideDefault(typeof(TestController).GetTypeInfo().GetMethod("GetByGuid").GetParameters()[1]);

            result.Should().BeOfType<ToHeaderAttribute>();
        }

        [Test]
        public void it_should_provide_a_parameter_target_for_int()
        {
            var result = _selector.ProvideDefault(typeof(TestController).GetTypeInfo().GetMethod("Add").ReturnParameter);

            result.Should().BeOfType<ToBodyAttribute>();
        }

        [Test]
        public void it_should_provide_a_parameter_target_for_Id()
        {
            var result = _selector.ProvideDefault(typeof(TestController).GetTypeInfo().GetMethod("GetBySomeId").GetParameters()[1]);

            result.Should().BeOfType<ToHeaderAttribute>();
        }

        [Test]
        public void it_should_provide_a_parameter_target_for_object()
        {
            var result = _selector.ProvideDefault(typeof(TestController).GetTypeInfo().GetMethod("GetSomething").GetParameters()[0]);

            result.Should().BeOfType<ToBodyAttribute>();
        }

        [SetUp]
        public void Setup()
        {
            _selector = new DefaultValueRelationSelector();
        }

        [TearDown]
        public void Teardown()
        {
            _selector = null;
        }
    }
}