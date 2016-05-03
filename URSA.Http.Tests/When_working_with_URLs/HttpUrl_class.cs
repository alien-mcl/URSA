using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Http;

namespace When_working_with_URLs
{
    [TestClass]
    public class HttpUrl_class
    {
        private HttpUrl _url;

        [TestMethod]
        public void should_add_segment_correctly()
        {
            _url.AddSegment("you").AddSegment("imagine").ToString().Should().Be("http://temp.uri/whatever/path/you/imagine?with=query#and-fragment");
        }

        [TestMethod]
        public void should_remove_last_segment_correctly()
        {
            _url.RemoveSegment().ToString().Should().Be("http://temp.uri/whatever?with=query#and-fragment");
        }

        [TestMethod]
        public void should_remove_all_segments_correctly()
        {
            _url.RemoveSegment().RemoveSegment().RemoveSegment().ToString().Should().Be("http://temp.uri/?with=query#and-fragment");
        }

        [TestMethod]
        public void should_replace_fragment_correctly()
        {
            _url.WithFragment("with-fragment", ExistingFragment.Replace).ToString().Should().Be("http://temp.uri/whatever/path?with=query#with-fragment");
        }

        [TestMethod]
        public void should_append_fragment_correctly()
        {
            _url.WithFragment("s", ExistingFragment.Append).ToString().Should().Be("http://temp.uri/whatever/path?with=query#and-fragments");
        }

        [TestMethod]
        public void should_remove_fragment_correctly()
        {
            _url.WithFragment(null).ToString().Should().Be("http://temp.uri/whatever/path?with=query");
        }

        [TestMethod]
        public void should_add_fragment_moving_existing_one_as_segment_correctly()
        {
            _url.WithFragment("and-new-fragment", ExistingFragment.MoveAsSegment).ToString().Should().Be("http://temp.uri/whatever/path/and-fragment?with=query#and-new-fragment");
        }

        [TestMethod]
        public void should_give_same_instance_if_no_fragments_are_given()
        {
            var expected = new HttpUrl(true, "http://temp.uri/", "http", "temp.uri", 80, "/", null, null);
            expected.WithFragment(null).Should().BeSameAs(expected);
        }

        [TestMethod]
        public void should_set_new_fragment_if_there_was_no_existing_one()
        {
            new HttpUrl(true, "http://temp.uri/", "http", "temp.uri", 80, "/", null, null).WithFragment("new-fragment").ToString().Should().Be("http://temp.uri/#new-fragment");
        }

        [TestMethod]
        public void should_throw_when_fragment_already_exists()
        {
            _url.Invoking(instance => instance.WithFragment("new-fragment")).ShouldThrow<InvalidOperationException>();
        }

        [TestInitialize]
        public void Setup()
        {
            var parameters = new ParametersCollection("&", "=");
            parameters.AddValue("with", "query");
            _url = new HttpUrl(true, "http://temp.uri/whatever/path?with=query#and-fragment", "http", "temp.uri", 80, "/whatever/path", parameters, "and-fragment", "whatever", "path");
        }

        [TestCleanup]
        public void Teardown()
        {
            _url = null;
        }
    }
}