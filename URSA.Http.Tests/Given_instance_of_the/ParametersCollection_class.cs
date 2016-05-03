using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Http;

namespace Given_instance_of_the
{
    [TestClass]
    public class ParametersCollection_class
    {
        private ParametersCollection _parameters;

        [TestMethod]
        public void it_should_add_a_key_with_empty_value()
        {
            _parameters.AddValue("key", String.Empty);
            _parameters.AddValue("another", String.Empty);

            _parameters.Should().HaveCount(2).And.Subject.First().Should().Be(new KeyValuePair<string, string>("key", String.Empty));
        }

        [TestMethod]
        public void it_should_set_a_value()
        {
            _parameters["key"] = "value1";

            _parameters["key"] = "value2";

            _parameters["key"].Should().Be("value2");
        }

        [TestMethod]
        public void it_should_add_a_multiple_values_for_a_key()
        {
            _parameters.AddValue("key", "value1");
            _parameters.AddValue("key", "value2");

            _parameters.GetValues("key").Should().HaveCount(2).And.Subject.First().Should().Be("value1");
        }

        [TestMethod]
        public void it_should_add_value_as_a_key_if_no_key_is_given()
        {
            _parameters.AddValue(String.Empty, "value");

            _parameters.GetValues("value").Should().HaveCount(1).And.Subject.First().Should().Be(String.Empty);
        }

        [TestMethod]
        public void it_should_iterate_through_keys_and_values_correctly()
        {
            var expected = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("key1", "value1"),
                    new KeyValuePair<string, string>("key1", "value2"),
                    new KeyValuePair<string, string>("key2", "value1"),
                    new KeyValuePair<string, string>("key2", "value2")
                };

            foreach (var entry in expected)
            {
                _parameters.AddValue(entry.Key, entry.Value);
            }

            int iteratedEntries = 0;
            foreach (var entry in _parameters)
            {
                expected.Should().Contain(entry);
                iteratedEntries++;
            }

            iteratedEntries.Should().Be(expected.Count);
        }

        [TestInitialize]
        public void Setup()
        {
            _parameters = new ParametersCollection("&", "=");
        }

        [TestCleanup]
        public void Teardown()
        {
            _parameters = null;
        }
    }
}
