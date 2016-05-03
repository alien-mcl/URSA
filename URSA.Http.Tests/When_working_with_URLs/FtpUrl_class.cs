using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Http;

namespace When_working_with_URLs
{
    [TestClass]
    public class FtpUrl_class
    {
        private FtpUrl _url;

        [TestMethod]
        public void should_add_segment_correctly()
        {
            _url.AddSegment("you").AddSegment("imagine").ToString().Should().Be("ftp://temp.uri/whatever/path/you/imagine;type=i");
        }

        [TestMethod]
        public void should_remove_last_segment_correctly()
        {
            _url.RemoveSegment().ToString().Should().Be("ftp://temp.uri/whatever;type=i");
        }

        [TestMethod]
        public void should_remove_all_segments_correctly()
        {
            _url.RemoveSegment().RemoveSegment().RemoveSegment().ToString().Should().Be("ftp://temp.uri/;type=i");
        }

        [TestInitialize]
        public void Setup()
        {
            var parameters = new ParametersCollection(";", "=");
            parameters.AddValue("type", "i");
            _url = new FtpUrl("ftp://temp.uri/whatever/path;type=i", "ftp", null, null, "temp.uri", 21, "/whatever/path", parameters, "whatever", "path");
        }

        [TestCleanup]
        public void Teardown()
        {
            _url = null;
        }
    }
}