#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using URSA.Security;
using URSA.Web.Http.Description.Tests.Data;
using URSA.Web.Mapping;

namespace URSA.Web.Http.Description.Tests
{
    [ExcludeFromCodeCoverage]
    [Route("api/person")]
    public class TestController : IWriteController<Person, Guid>
    {
        public IResponseInfo Response { get; set; }

        /// <summary>Gets all persons.</summary>
        /// <param name="totalItems">Total items in collection.</param>
        /// <param name="page">Page of the collection. Use 0 for all of the entities.</param>
        /// <param name="pageSize">Page size. Ignored when <paramref name="page" /> is set to 0.</param>
        /// <returns>Collection of entities.</returns>
        public IEnumerable<Person> List(out int totalItems, int page = 0, int pageSize = 0)
        {
            totalItems = 0;
            return new Person[0];
        }

        /// <summary>Gets the given person.</summary>
        /// <param name="id">Identifier of the person to retrieve.</param>
        /// <returns>Instance of the <see cref="Person" /> if matching <paramref name="id" />; otherwise <b>null</b>.</returns>
        public Person Get(Guid id)
        {
            return null;
        }

        /// <summary>Creates the specified person which identifier is returned when created.</summary>
        /// <param name="person">The person.</param>
        public Guid Create(Person person)
        {
            return Guid.Empty;
        }

        [DenyClaim(ClaimTypes.Anonymous)]
        public void Update(Guid id, Person person)
        {
        }

        /// <summary>Deletes an entity.</summary>
        /// <param name="id">Identifier of the entity to be deleted.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when an identifier is not provided.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when an identifier is empty.</exception>
        /// <exception cref="URSA.Web.AccessDeniedException">Thrown when an identity is not authorized.</exception>
        public void Delete(Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            if (id == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException("id");
            }
        }

        public void SetRoles(Guid id, IEnumerable<string> roles)
        {
        }
    }
}