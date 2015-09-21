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

            description.Should().Be("Creates the specified person which id is returned when created.");
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