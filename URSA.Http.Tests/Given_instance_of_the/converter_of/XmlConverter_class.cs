using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Testing;
using URSA.Web.Http.Tests.Data;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class XmlConverter_class : ConverterTest<XmlConverter>
    {
        [TestMethod]
        public void it_should_deserialize_message_as_an_Person()
        {
            Person body = new Person() { Id = 1, FirstName = "Tester", LastName = "Test", Roles = new[] { "Role" } };
            var result = ConvertTo<Person>("POST", "PostString", XmlConverter.ApplicationXml, SerializeObject(body));

            result.Should().NotBeNull();
            result.Id.Should().Be(body.Id);
            result.FirstName.Should().Be(body.FirstName);
            result.LastName.Should().Be(body.LastName);
            result.Roles.Should().HaveCount(body.Roles.Length);
            result.Roles.First().Should().Be(body.Roles.First());
        }

        [TestMethod]
        public void it_should_deserialize_message_as_an_array_of_Persons()
        {
            Person[] body = new[]
            {
                new Person() { Id = 2, FirstName = "Tester", LastName = "Test 1", Roles = new[] { "Role 1" } },
                new Person() { Id = 2, FirstName = "Tester", LastName = "Test 2", Roles = new[] { "Role 2" } }
            };
            var result = ConvertTo<Person[]>("POST", "PostStrings", XmlConverter.ApplicationXml, SerializeObject(body));

            result.Should().NotBeNull();
            result.Should().HaveCount(body.Length);
            result.First().Id.Should().Be(body.First().Id);
            result.First().FirstName.Should().Be(body.First().FirstName);
            result.First().LastName.Should().Be(body.First().LastName);
            result.First().Roles.Should().HaveCount(body.First().Roles.Length);
            result.First().Roles.First().Should().Be(body.First().Roles.First());
            result.Last().Id.Should().Be(body.Last().Id);
            result.Last().FirstName.Should().Be(body.Last().FirstName);
            result.Last().LastName.Should().Be(body.Last().LastName);
            result.Last().Roles.Should().HaveCount(body.Last().Roles.Length);
            result.Last().Roles.First().Should().Be(body.Last().Roles.First());
        }

        [TestMethod]
        public void it_should_serialize_a_Person_to_message()
        {
            Person body = new Person() { Id = 2, FirstName = "Tester", LastName = "Test 1", Roles = new[] { "Role 1" } };
            var content = ConvertFrom<Person>("POST", "PostStrings", "application/xml", body);

            content.Should().Be(SerializeObject(body));
        }

        [TestMethod]
        public void it_should_serialize_array_of_Persons_to_message()
        {
            Person[] body = new[]
            {
                new Person() { Id = 2, FirstName = "Tester", LastName = "Test 1", Roles = new[] { "Role 1" } },
                new Person() { Id = 2, FirstName = "Tester", LastName = "Test 2", Roles = new[] { "Role 2" } }
            };
            var content = ConvertFrom<Person[]>("POST", "PostStrings", "application/xml", body);

            content.Should().Be(SerializeObject(body));
        }

        private string SerializeObject<T>(T obj)
        {
            var serializer = new DataContractSerializer(typeof(T));
            var buffer = new MemoryStream();
            serializer.WriteObject(buffer, obj);
            return Encoding.UTF8.GetString(buffer.ToArray());
        }
    }
}