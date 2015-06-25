using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net;
using URSA.Web;
using URSA.Web.Description;
using URSA.Web.Http;
using URSA.Web.Http.Tests.Data;
using URSA.Web.Http.Tests.Testing;
using URSA.Web.Tests;

namespace Given_instance_of_the.ResponseComposer_class
{
    [TestClass]
    public class when_dealing_with_CRUD_controller : ResponseComposerTest<CrudController>
    {
        [TestMethod]
        public void it_should_handle_List_request_correctly()
        {
            object[] arguments = { 1, 2 };
            var expected = new Person[] { new Person() { Id = 1 } };
            Converter.Setup(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()));

            var result = Composer.ComposeResponse(CreateRequestMapping("List", expected, arguments), expected, arguments);

            result.Should().BeOfType<ObjectResponseInfo<Person[]>>();
            result.Status.Should().Be(HttpStatusCode.OK);
            Converter.Verify(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()), Times.Once);
        }

        [TestMethod]
        public void it_should_handle_Read_request_correctly()
        {
            object[] arguments = { 1 };
            Person expected = null;
            Converter.Setup(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()));

            var result = Composer.ComposeResponse(CreateRequestMapping("Get", expected, arguments), expected, arguments);

            result.Should().BeOfType<StringResponseInfo>();
            result.Status.Should().Be(HttpStatusCode.NotFound);
            Converter.Verify(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()), Times.Never);
        }

        [TestMethod]
        public void it_should_return_404_not_found_when_reading_inexisting_resource()
        {
            object[] arguments = { 2 };
            var expected = new Person() { Id = (int)arguments[0] };
            Converter.Setup(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()));

            var result = Composer.ComposeResponse(CreateRequestMapping("Get", expected, arguments), expected, arguments);

            result.Should().BeOfType<ObjectResponseInfo<Person>>();
            result.Status.Should().Be(HttpStatusCode.OK);
            Converter.Verify(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()), Times.Once);
        }

        [TestMethod]
        public void it_should_handle_Create_request_correctly()
        {
            object[] arguments = { 1, new Person() };
            bool? expected = true;
            string callUri;
            var controllerDescription = new ControllerInfo<CrudController>(
                new Uri("api/test", UriKind.Relative),
                Controller.GetType().GetMethod("Get").ToOperationInfo("api/test", Verb.GET, out callUri, new[] { arguments[0] }),
                Controller.GetType().GetMethod("Create").ToOperationInfo("api/test", Verb.POST, out callUri, arguments));
            ControllerDescriptionBuilder.As<IControllerDescriptionBuilder>().Setup(instance => instance.BuildDescriptor()).Returns(controllerDescription);

            var result = Composer.ComposeResponse(CreateRequestMapping("Create", Verb.POST, expected, arguments), expected, arguments);

            result.Should().BeOfType<StringResponseInfo>();
            result.Status.Should().Be(HttpStatusCode.Created);
            result.Headers.Should().ContainKey("Location").WhichValue.Should().Be("/api/test/get/id/1");
            Converter.Verify(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()), Times.Never);
        }

        [TestMethod]
        public void it_should_return_409_conflict_when_creating_resource_already_exists()
        {
            object[] arguments = { 1, new Person() };
            bool? expected = false;
            string callUri;
            var controllerDescription = new ControllerInfo<CrudController>(
                new Uri("api/test", UriKind.Relative),
                Controller.GetType().GetMethod("Get").ToOperationInfo("api/test", Verb.GET, out callUri, new[] { arguments[0] }),
                Controller.GetType().GetMethod("Create").ToOperationInfo("api/test", Verb.POST, out callUri, arguments));
            ControllerDescriptionBuilder.As<IControllerDescriptionBuilder>().Setup(instance => instance.BuildDescriptor()).Returns(controllerDescription);

            var result = Composer.ComposeResponse(CreateRequestMapping("Create", Verb.POST, expected, arguments), expected, arguments);

            result.Should().BeOfType<StringResponseInfo>();
            result.Status.Should().Be(HttpStatusCode.Conflict);
            result.Headers.Should().NotContainKey("Location");
            Converter.Verify(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()), Times.Never);
        }

        [TestMethod]
        public void it_should_handle_Update_request_correctly()
        {
            object[] arguments = { 1, new Person() { Id = 1 } };
            bool? expected = true;
            Converter.Setup(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()));

            var result = Composer.ComposeResponse(CreateRequestMapping("Update", expected, arguments), expected, arguments);

            result.Should().BeOfType<StringResponseInfo>();
            result.Status.Should().Be(HttpStatusCode.NoContent);
            Converter.Verify(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()), Times.Never);
        }

        [TestMethod]
        public void it_should_return_404_not_found_when_updating_inexisting_resource()
        {
            object[] arguments = { 2, new Person() { Id = 2 } };
            bool? expected = null;
            Converter.Setup(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()));

            var result = Composer.ComposeResponse(CreateRequestMapping("Update", expected, arguments), expected, arguments);

            result.Should().BeOfType<StringResponseInfo>();
            result.Status.Should().Be(HttpStatusCode.NotFound);
            Converter.Verify(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()), Times.Never);
        }

        [TestMethod]
        public void it_should_handle_Delete_request_correctly()
        {
            object[] arguments = { 1 };
            bool? expected = true;
            Converter.Setup(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()));

            var result = Composer.ComposeResponse(CreateRequestMapping("Delete", expected, arguments), expected, arguments);

            result.Should().BeOfType<StringResponseInfo>();
            result.Status.Should().Be(HttpStatusCode.NoContent);
            Converter.Verify(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()), Times.Never);
        }

        [TestMethod]
        public void it_should_return_404_not_found_when_deleting_inexisting_resource()
        {
            object[] arguments = { 1 };
            bool? expected = null;
            Converter.Setup(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()));

            var result = Composer.ComposeResponse(CreateRequestMapping("Delete", expected, arguments), expected, arguments);

            result.Should().BeOfType<StringResponseInfo>();
            result.Status.Should().Be(HttpStatusCode.NotFound);
            Converter.Verify(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()), Times.Never);
        }
    }
}