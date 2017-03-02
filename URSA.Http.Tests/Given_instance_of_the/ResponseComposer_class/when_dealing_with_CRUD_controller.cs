using FluentAssertions;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using NUnit.Framework;
using URSA;
using URSA.Web;
using URSA.Web.Description;
using URSA.Web.Http;
using URSA.Web.Http.Tests.Data;
using URSA.Web.Http.Tests.Testing;
using URSA.Web.Tests;

namespace Given_instance_of_the.ResponseComposer_class
{
    [TestFixture]
    public class when_dealing_with_CRUD_controller : ResponseComposerTest<CrudController>
    {
        [Test]
        public void it_should_handle_List_request_correctly()
        {
            object[] arguments = { 1, 1, 2 };
            var expected = new[] { new Person() { Key = 1 } };
            Converter.Setup(instance => instance.ConvertFrom(It.Is<IEnumerable>(collection => collection.Cast<Person>().Contains(expected[0])), It.IsAny<IResponseInfo>()));

            var result = Composer.ComposeResponse(CreateRequestMapping("List", arguments), expected, arguments);

            result.Should().NotBeNull();
            result.GetType().GetTypeInfo().IsGenericType.Should().BeTrue();
            result.GetType().GetGenericTypeDefinition().Should().Be(typeof(ObjectResponseInfo<>));
            typeof(IEnumerable<Person>).IsAssignableFrom(result.GetType().GetGenericArguments()[0]).Should().BeTrue();
            result.Status.Should().Be(HttpStatusCode.PartialContent);
            Converter.Verify(instance => instance.ConvertFrom(It.Is<IEnumerable>(collection => collection.Cast<Person>().Contains(expected[0])), It.IsAny<IResponseInfo>()), Times.Once);
        }

        [Test]
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

        [Test]
        public void it_should_return_404_not_found_when_reading_inexisting_resource()
        {
            object[] arguments = { 2 };
            var expected = new Person() { Key = (int)arguments[0] };
            Converter.Setup(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()));

            var result = Composer.ComposeResponse(CreateRequestMapping("Get", arguments), expected, arguments);

            result.Should().BeOfType<ObjectResponseInfo<Person>>();
            result.Status.Should().Be(HttpStatusCode.OK);
            Converter.Verify(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()), Times.Once);
        }

        [Test]
        public void it_should_handle_Create_request_correctly()
        {
            var person = new Person() { Key = 1 };
            string callUri;
            var controllerDescription = new ControllerInfo<CrudController>(
                null,
                (HttpUrl)UrlParser.Parse("api/test"),
                Controller.GetType().GetMethod("Get").ToOperationInfo("api/test", Verb.GET, out callUri, person.Key),
                Controller.GetType().GetMethod("Create").ToOperationInfo("api/test", Verb.POST, out callUri, person));
            ControllerDescriptionBuilder.As<IControllerDescriptionBuilder>().Setup(instance => instance.BuildDescriptor()).Returns(controllerDescription);

            var result = Composer.ComposeResponse(CreateRequestMapping("Create", Verb.POST, person), person.Key, person);

            result.Should().BeOfType<StringResponseInfo>();
            result.Status.Should().Be(HttpStatusCode.Created);
            result.Headers.Should().ContainKey("Location").WhichValue.Should().Be("/api/test/" + person.Key);
            Converter.Verify(instance => instance.ConvertFrom(person.Key, It.IsAny<IResponseInfo>()), Times.Never);
        }

        [Test]
        public void it_should_handle_Update_request_correctly()
        {
            object[] arguments = { 1, new Person() { Key = 1 } };
            bool? expected = true;
            Converter.Setup(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()));

            var result = Composer.ComposeResponse(CreateRequestMapping("Update", arguments), expected, arguments);

            result.Should().BeOfType<StringResponseInfo>();
            result.Status.Should().Be(HttpStatusCode.NoContent);
            Converter.Verify(instance => instance.ConvertFrom(expected, It.IsAny<IResponseInfo>()), Times.Never);
        }

        [Test]
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