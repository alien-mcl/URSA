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

            var result = Composer.ComposeResponse(CreateRequestMapping("List", arguments), expected, arguments);

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

            var result = Composer.ComposeResponse(CreateRequestMapping("Get", arguments), expected, arguments);

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

            var result = Composer.ComposeResponse(CreateRequestMapping("Get", arguments), expected, arguments);

            result.Should().BeOfType<ObjectResponseInfo<Person>>();
            result.Status.Should().Be(HttpStatusCode.OK);
            Converter.Verify(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()), Times.Once);
        }

        [TestMethod]
        public void it_should_handle_Create_request_correctly()
        {
            var person = new Person() { Id = 1 };
            string callUri;
            var controllerDescription = new ControllerInfo<CrudController>(
                null,
                new Uri("api/test", UriKind.Relative),
                Controller.GetType().GetMethod("Get").ToOperationInfo("api/test", Verb.GET, out callUri, person.Id),
                Controller.GetType().GetMethod("Create").ToOperationInfo("api/test", Verb.POST, out callUri, person));
            ControllerDescriptionBuilder.As<IControllerDescriptionBuilder>().Setup(instance => instance.BuildDescriptor()).Returns(controllerDescription);

            var result = Composer.ComposeResponse(CreateRequestMapping("Create", Verb.POST, person), person.Id, person);

            result.Should().BeOfType<StringResponseInfo>();
            result.Status.Should().Be(HttpStatusCode.Created);
            result.Headers.Should().ContainKey("Location").WhichValue.Should().Be("/api/test/get/id/" + person.Id);
            Converter.Verify(instance => instance.ConvertFrom(person.Id, It.IsAny<IResponseInfo>()), Times.Never);
        }

        [TestMethod]
        public void it_should_handle_Update_request_correctly()
        {
            object[] arguments = { 1, new Person() { Id = 1 } };
            bool? expected = true;
            Converter.Setup(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()));

            var result = Composer.ComposeResponse(CreateRequestMapping("Update", arguments), expected, arguments);

            result.Should().BeOfType<StringResponseInfo>();
            result.Status.Should().Be(HttpStatusCode.NoContent);
            Converter.Verify(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()), Times.Never);
        }

        [TestMethod]
        public void it_should_handle_Delete_request_correctly()
        {
            object[] arguments = { 1 };
            bool? expected = true;
            Converter.Setup(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()));

            var result = Composer.ComposeResponse(CreateRequestMapping("Delete", arguments), expected, arguments);

            result.Should().BeOfType<StringResponseInfo>();
            result.Status.Should().Be(HttpStatusCode.NoContent);
            Converter.Verify(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()), Times.Never);
        }
    }
}