using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RomanticWeb;
using URSA.Web.Http.Description;

namespace URSA.Http.Description.Tests.Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class EntryPointDescriptionController_class
    {
        [TestMethod]
        public void it_should_provide_file_name_from_Url_fragment()
        {
            var expected = "name";
            var controller = CreateControllerInstance(new Uri("/test#" + expected, UriKind.Relative));

            controller.FileName.Should().Be(expected);
        }

        [TestMethod]
        public void it_should_provide_file_name_from_Urls_last_segment()
        {
            var expected = "name";

            var controller = CreateControllerInstance(new Uri("/test/" + expected, UriKind.Relative));

            controller.FileName.Should().Be(expected);
        }

        private EntryPointDescriptionController CreateControllerInstance(Uri entryPoint)
        {
            var builder = new Mock<IApiEntryPointDescriptionBuilder>(MockBehavior.Strict);
            builder.SetupSet(instance => instance.EntryPoint = entryPoint);
            builder.SetupGet(instance => instance.EntryPoint).Returns(entryPoint);
            return new EntryPointDescriptionController(entryPoint, new Mock<IEntityContext>(MockBehavior.Strict).Object, builder.Object);
        }
    }
}