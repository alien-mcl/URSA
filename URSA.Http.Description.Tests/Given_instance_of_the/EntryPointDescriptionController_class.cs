using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using URSA;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.Entities;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class EntryPointDescriptionController_class
    {
        [Test]
        public void it_should_provide_file_name_from_Url_fragment()
        {
            var expected = "name";
            var controller = CreateControllerInstance((HttpUrl)UrlParser.Parse("/test#" + expected));

            controller.FileName.Should().Be(expected);
        }

        [Test]
        public void it_should_provide_file_name_from_Urls_last_segment()
        {
            var expected = "name";

            var controller = CreateControllerInstance((HttpUrl)UrlParser.Parse("/test/" + expected));

            controller.FileName.Should().Be(expected);
        }

        private EntryPointDescriptionController CreateControllerInstance(HttpUrl entryPoint)
        {
            var builder = new Mock<IApiEntryPointDescriptionBuilder>(MockBehavior.Strict);
            builder.SetupSet(instance => instance.EntryPoint = entryPoint);
            builder.SetupGet(instance => instance.EntryPoint).Returns(entryPoint);
            return new EntryPointDescriptionController(
                entryPoint,
                new Mock<IEntityContextProvider>(MockBehavior.Strict).Object,
                builder.Object,
                new[] { new Mock<IHttpControllerDescriptionBuilder>(MockBehavior.Strict).Object });
        }
    }
}