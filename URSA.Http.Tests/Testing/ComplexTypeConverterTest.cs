using System.Linq;
using FluentAssertions;
using URSA.Web.Converters;
using URSA.Web.Http.Tests.Data;

namespace URSA.Web.Http.Testing
{
    public abstract class ComplexTypeConverterTest<T> : ConverterTest<T, Person> where T : class, IConverter
    {
        private static readonly Person Entity = new Person() { Id = 1, FirstName = "Tester", LastName = "Test", Roles = new[] { "Role" } };
        private static readonly Person[] Entities =
        {
            new Person() { Id = 2, FirstName = "Tester", LastName = "Test 1", Roles = new[] { "Role 1" } },
            new Person() { Id = 2, FirstName = "Tester", LastName = "Test 2", Roles = new[] { "Role 2" } }
        };

        protected override Person SingleEntity { get { return Entity; } }

        protected override Person[] MultipleEntities { get { return Entities; } }

        protected override string SingleEntityBody { get { return SerializeObject(Entity); } }

        protected override string MultipleEntitiesBody { get { return SerializeObject(Entities); } }

        protected override void AssertSingleEntity(Person result)
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(SingleEntity.Id);
            result.FirstName.Should().Be(SingleEntity.FirstName);
            result.LastName.Should().Be(SingleEntity.LastName);
            result.Roles.Should().HaveCount(SingleEntity.Roles.Length);
            result.Roles.First().Should().Be(SingleEntity.Roles.First());
        }

        protected override void AssertMultipleEntities(Person[] result)
        {
            result.Should().NotBeNull();
            result.Should().HaveCount(MultipleEntities.Length);
            result.First().Id.Should().Be(MultipleEntities.First().Id);
            result.First().FirstName.Should().Be(MultipleEntities.First().FirstName);
            result.First().LastName.Should().Be(MultipleEntities.First().LastName);
            result.First().Roles.Should().HaveCount(MultipleEntities.First().Roles.Length);
            result.First().Roles.First().Should().Be(MultipleEntities.First().Roles.First());
            result.Last().Id.Should().Be(MultipleEntities.Last().Id);
            result.Last().FirstName.Should().Be(MultipleEntities.Last().FirstName);
            result.Last().LastName.Should().Be(MultipleEntities.Last().LastName);
            result.Last().Roles.Should().HaveCount(MultipleEntities.Last().Roles.Length);
            result.Last().Roles.First().Should().Be(MultipleEntities.Last().Roles.First());
        }

        protected abstract string SerializeObject<TI>(TI obj);
    }
}