using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Description.Http;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the
{
    [TestClass]
    public class QueryStringList_class
    {
        [TestMethod]
        public void it_should_build_uri_template_properly()
        {
            var segments = new UriTemplate.QueryStringList(null, false);
            segments.Add("id={?id}", new FromQueryStringAttribute("&id={?id}"), typeof(CrudController).GetMethod("Update").GetParameters()[0]);
            segments.Add("person={?person}", new FromQueryStringAttribute("&person={?person}"), typeof(CrudController).GetMethod("Update").GetParameters()[1]);

            var result = segments.ToString();

            result.Should().Be("?id={id}{&person}");
        }

        [TestMethod]
        public void it_should_build_uri_regex_properly()
        {
            var segments = new UriTemplate.QueryStringList(null, true);
            segments.Add("id=[^&]+", new FromQueryStringAttribute("&id={?id}"), typeof(CrudController).GetMethod("Update").GetParameters()[0]);
            segments.Add("person=[^&]+", new FromQueryStringAttribute("&person={?person}"), typeof(CrudController).GetMethod("Update").GetParameters()[1]);

            var result = segments.ToString();

            result.Should().Be("([?&](id|person)=[^&]*){0,}");
        }
    }
}