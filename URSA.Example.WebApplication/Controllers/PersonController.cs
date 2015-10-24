using System;
using System.Collections.Generic;
using System.Linq;
using URSA.Example.WebApplication.Data;
using URSA.Web;
using URSA.Web.Http.Description.Mapping;

namespace URSA.Example.WebApplication.Controllers
{
    /// <summary>Provides a basic person handling.</summary>
    public class PersonController : IWriteController<Person, Guid>
    {
        private static readonly IList<Person> Repository = new List<Person>();

        /// <inheritdoc />
        public IResponseInfo Response { get; set; }

        /// <summary>Gets all persons.</summary>
        /// <param name="skip">Skips top <paramref name="skip" /> elements of the collection.</param>
        /// <param name="take">Takes top <paramref name="take" /> elements of the collection. Use 0 for all of the entities.</param>
        /// <returns>Collection of entities.</returns>
        public IEnumerable<Person> List([LinqServerBehavior(LinqOperations.Skip)] int skip = 0, [LinqServerBehavior(LinqOperations.Take)] int take = 0)
        {
            return Repository;
        }

        /// <summary>Gets the person with identifier of <paramref name="id" />.</summary>
        /// <param name="id">Identifier of the person to retrieve.</param>
        /// <returns>Instance of the <see cref="Person" /> if matching <paramref name="id" />; otherwise <b>null</b>.</returns>
        public Person Get(Guid id)
        {
            return (from entity in Repository where entity.Key == id select entity).FirstOrDefault();
        }

        /// <summary>Creates the specified person.</summary>
        /// <param name="person">The person.</param>
        /// <returns>Identifier of newly created person.</returns>
        public Guid Create(Person person)
        {
            person.Key = Guid.NewGuid();
            Repository.Add(person);
            return person.Key;
        }

        /// <summary>Updates the specified person.</summary>
        /// <param name="id">The identifier of the person to be updated.</param>
        /// <param name="person">The person.</param>
        public void Update(Guid id, Person person)
        {
            (Repository[GetIndex(id)] = person).Key = id;
        }

        /// <summary>Deletes a person.</summary>
        /// <param name="id">Identifier of the person to be deleted.</param>
        public void Delete(Guid id)
        {
            Repository.RemoveAt(GetIndex(id));
        }

        /// <summary>Adds the given <paramref name="roles" /> to the user with given <paramref name="id" />.</summary>
        /// <param name="id">The identifier.</param>
        /// <param name="roles">The roles.</param>
        public void SetRoles(Guid id, IEnumerable<string> roles)
        {
            var user = Repository[GetIndex(id)];
            user.Roles.Clear();
            roles.ForEach(role => user.Roles.Add(role));
        }

        private int GetIndex(Guid id)
        {
            int index = -1;
            Repository.Where((entity, entityIndex) => (entity.Key == id) && ((index = entityIndex) != -1)).FirstOrDefault();
            if (index == -1)
            {
                throw new NotFoundException(String.Format("Person with id of '{0}' does not exist.", id));
            }

            return index;
        }
    }
}