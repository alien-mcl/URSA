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
        public void it_should_add_parameter()
        {
            var parameters = new HeaderParameterCollection();

            parameters.Add("test");

            parameters["test"].Should().NotBeNull();
        }

        [TestMethod]
        public void it_should_remove_parameter_by_name()
        {
            var parameters = new HeaderParameterCollection();
            parameters.Add("test");

            parameters.Remove("test");

            parameters["test"].Should().BeNull();
        }

        [TestMethod]
        public void it_should_remove_parameter()
        {
            var parameters = new HeaderParameterCollection();
            parameters.Add("test", 1);

            parameters.Remove(new HeaderParameter("test", 1));

            parameters["test"].Should().BeNull();
        }

        [TestMethod]
        public void it_should_return_null_if_no_parameter_of_given_key_is_found()
        {
            new HeaderParameterCollection()["test"].Should().BeNull();
        }

        [TestMethod]
        public void it_should_throw_when_setting_a_parameter_without_passing_a_name()
        {
            new HeaderParameterCollection().Invoking(parameters => parameters[null] = null).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("parameter");
        }

        [TestMethod]
        public void it_should_throw_when_setting_a_parameter_with_an_empty_name()
        {
            new HeaderParameterCollection().Invoking(parameters => parameters[String.Empty] = null).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("parameter");
        }

        [TestMethod]
        public void it_should_throw_when_getting_a_parameter_without_passing_a_name()
        {
            new HeaderParameterCollection().Invoking(parameters => { var test = parameters[null]; }).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("parameter");
        }

        [TestMethod]
        public void it_should_throw_when_adding_a_parametre_without_passing_a_name()
        {
            new HeaderParameterCollection().Invoking(parameters => parameters.Add((HeaderParameter)null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("parameter");
        }

        [TestMethod]
        public void it_should_throw_when_removing_a_parametre_without_passing_a_name()
        {
            new HeaderParameterCollection().Invoking(parameters => parameters.Remove((HeaderParameter)null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("parameter");
        }

        [TestMethod]
        public void it_should_throw_when_setting_parameter_with_incorrect_name()
        {
            new HeaderParameterCollection().Invoking(parameters => parameters["test"] = new HeaderParameter("some")).ShouldThrow<InvalidOperationException>();
        }
    }
}