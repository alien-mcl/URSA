using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Description.Http;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the
{
    [TestClass]
    public class SegmentList_class
    {
        [TestMethod]
        public void it_should_build_uri_template_properly()
        {
            var segments = new UriTemplate.SegmentList(null, false);
            segments.Add("/api", null, typeof(CrudController));
            segments.Add("person", new RouteAttribute("/person"), typeof(CrudController).GetMethod("Update"));
            segments.Add("id/{?id}", new FromUriAttribute("/id/{?value}"), typeof(CrudController).GetMethod("Update").GetParameters()[0]);
            segments.Add("person/{?person}", new FromUriAttribute("/person/{?value}"), typeof(CrudController).GetMethod("Update").GetParameters()[1], true);

            var result = segments.ToString();

            result.Should().Be("/api/person/id/{?id}/person/{?person}");
        }

        [TestMethod]
        public void it_should_build_uri_regex_properly()
        {
            var segments = new UriTemplate.SegmentList(null, true);
            segments.Add("api", null, typeof(CrudController));
            segments.Add("person", new RouteAttribute("/person"), typeof(CrudController).GetMethod("Update"));
            segments.Add("id/[^/?]+", new FromUriAttribute("/id/{?value}"), typeof(CrudController).GetMethod("Update").GetParameters()[0]);
            segments.Add("person/[^/?]+", new FromUriAttribute("/person/{?value}"), typeof(CrudController).GetMethod("Update").GetParameters()[1], true);

            var result = segments.ToString();

            result.Should().Be("/api/person/id/[^/?]+(/person/[^/?]+)?");
        }
    }
}