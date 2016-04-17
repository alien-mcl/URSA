using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Http;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class Verb_class
    {
        [TestMethod]
        public void it_should_throw_when_no_verb_is_passed_for_parsing()
        {
            ((Verb)null).Invoking(_ => Verb.Parse(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("detectedVerb");
        }

        [TestMethod]
        public void it_should_throw_when_a_verb_passed_for_parsing_is_empty()
        {
            ((Verb)null).Invoking(_ => Verb.Parse(String.Empty)).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("detectedVerb");
        }

        [TestMethod]
        public void it_should_parse_get_verb()
        {
            var result = Verb.Parse("get");

            result.Should().Be(Verb.GET);
        }

        [TestMethod]
        public void it_should_parse_Post_verb()
        {
            var result = Verb.Parse("Post");

            result.Should().Be(Verb.POST);
        }

        [TestMethod]
        public void it_should_parse_HEAD_verb()
        {
            var result = Verb.Parse("HEAD");

            result.Should().Be(Verb.HEAD);
        }

        [TestMethod]
        public void it_should_detect_that_one_verb_equals_another()
        {
            Verb.GET.Equals(new Verb("GET")).Should().BeTrue();
        }

        [TestMethod]
        public void it_should_detect_that_verbs_are_equal()
        {
            (Verb.GET == new Verb("GET")).Should().BeTrue();
        }

        [TestMethod]
        public void it_should_detect_that_verbs_are_unequal()
        {
            (Verb.GET != new Verb("POST")).Should().BeTrue();
        }
    }
}