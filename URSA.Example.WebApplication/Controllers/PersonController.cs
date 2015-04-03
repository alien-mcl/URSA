using System;
using System.Collections.Generic;
using URSA.Example.WebApplication.Data;
using URSA.Web;

namespace URSA.Example.WebApplication.Controllers
{
    /// <summary>Provides a basic person handling.</summary>
    public class PersonController : IController<Person>, IReadController<Person, Guid>, IWriteController<Person, Guid>
    {
        /// <inheritdoc />
        public IResponseInfo Response { get; set; }

        /// <summary>Gets all persons.</summary>
        /// <param name="page">Page of the collection. Use 0 for all of the entities.</param>
        /// <param name="pageSize">Page size. Ignored when <paramref name="page" /> is set to 0.</param>
        /// <returns>Collection of entities.</returns>
        public IEnumerable<Person> List(int page = 0, int pageSize = 0)
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
            id = Guid.Empty;
            return null;
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
    }
}