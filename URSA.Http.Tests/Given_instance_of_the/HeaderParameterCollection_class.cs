using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Http;

namespace Given_instance_of_the
{
    [TestClass]
    public class HeaderParameterCollection_class
    {
        [TestMethod]
        public void it_should_set_parameter_by_name()
        {
            var parameters = new HeaderParameterCollection();
            
            parameters["some"] = new HeaderParameter("some", "test");

            parameters["some"].Should().NotBeNull();
            parameters["some"].Value.Should().Be("test");
        }

        [TestMethod]
        public void it_should_override_old_parameter_when_setting_by_name()
        {
            var parameters = new HeaderParameterCollection();

            parameters["some"] = new HeaderParameter("some", "test");
            parameters["some"] = new HeaderParameter("some", "other");

            parameters["some"].Value.Should().Be("other");
        }

        [TestMethod]
        public void it_should_throw_when_setting_parameter_with_incorrect_name()
        {
            new HeaderParameterCollection().Invoking(parameters => parameters["test"] = new HeaderParameter("some")).ShouldThrow<InvalidOperationException>();
        }
    }
}