#pragma warning disable 1591
using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.Tests;
using URSA.Web.Http.Description.Tests.Data;

namespace Given_instance_of_the
{
    [TestClass]
    public class XmlDocProvider_class
    {
        private XmlDocProvider _provider;

        [TestMethod]
        public void it_should_extract_type_description()
        {
            var description = _provider.GetDescription(typeof(Person));

            description.Should().Be("Describes a person.");
        }

        [TestMethod]
        public void it_should_extract_property_description()
        {
            var description = _provider.GetDescription(typeof(Person).GetProperty("FirstName"));

            description.Should().Be("Gets or sets the person firstname.");
        }

        [TestMethod]
        public void it_should_extract_method_description()
        {
            var description = _provider.GetDescription(typeof(TestController).GetMethod("Create"));

            description.Should().Be("Creates the specified person which identifier is returned when created.");
        }

        [TestMethod]
        public void it_should_extract_method_exceptions_thrown()
        {
            var exceptions = _provider.GetExceptions(typeof(TestController).GetMethod("Delete"));

            exceptions.Should().HaveCount(3);
            exceptions.Should().Contain("System.ArgumentNullException");
            exceptions.Should().Contain("System.ArgumentOutOfRangeException");
            exceptions.Should().Contain("URSA.Web.AccessDeniedException");
        }

        [TestMethod]
        public void it_should_extract_method_parameter_description()
        {
            var method = typeof(TestController).GetMethod("Create");

            var description = _provider.GetDescription(method, method.GetParameters()[0]);

            description.Should().Be("The person.");
        }

        [TestInitialize]
        public void Setup()
        {
            _provider = new XmlDocProvider();
        }

        [TestCleanup]
        public void Teardown()
        {
            _provider = null;
        }
    }
}