using System;
using System.Collections.Generic;
using URSA.Example.WebApplication.Data;
using URSA.Web;
using URSA.Web.Http.Description.Mapping;

namespace URSA.Example.WebApplication.Controllers
{
    /// <summary>Provides a basic person handling.</summary>
    public class PersonController : IWriteController<Person, Guid>
    {
        /// <inheritdoc />
        public IResponseInfo Response { get; set; }

        /// <summary>Gets all persons.</summary>
        /// <param name="skip">Skips top <paramref name="skip" /> elements of the collection.</param>
        /// <param name="take">Takes top <paramref name="take" /> elements of the collection. Use 0 for all of the entities.</param>
        /// <returns>Collection of entities.</returns>
        public IEnumerable<Person> List([LinqServerBehavior(LinqOperations.Skip)] int skip = 0, [LinqServerBehavior(LinqOperations.Take)] int take = 0)
        {
            return new Person[0];
        }

        /// <summary>Gets the person with identifier of <paramref name="id" />.</summary>
        /// <param name="id">Identifier of the person to retrieve.</param>
        /// <returns>Instance of the <see cref="Person" /> if matching <paramref name="id" />; otherwise <b>null</b>.</returns>
        public Person Get(Guid id)
        {
            return null;
        }

        /// <summary>Creates the specified person.</summary>
        /// <param name="id">The identifier of the person created.</param>
        /// <param name="person">The person.</param>
        /// <returns><b>true</b> if the person was created or <b>false</b> if not; otherwise <b>null</b>.</returns>
        public bool? Create(out Guid id, Person person)
        {
            id = Guid.NewGuid();
            return true;
        }

        /// <summary>Updates the specified person.</summary>
        /// <param name="id">The identifier of the person to be updated.</param>
        /// <param name="person">The person.</param>
        /// <returns><b>true</b> if the person was updated or <b>false</b> if not; otherwise <b>null</b>.</returns>
        public bool? Update(Guid id, Person person)
        {
            return null;
        }

        /// <summary>Deletes a person.</summary>
        /// <param name="id">Identifier of the person to be deleted.</param>
        /// <returns><b>true</b> for success and <b>false</b> for failure; otherwise <b>null</b> when the instance does not exist.</returns>
        public bool? Delete(Guid id)
        {
            return null;
        }

        /// <summary>Adds the given <paramref name="roles" /> to the user with given <paramref name="id" />.</summary>
        /// <param name="id">The identifier.</param>
        /// <param name="roles">The roles.</param>
        public void SetRoles(Guid id, IEnumerable<string> roles)
        {
        }
    }
}