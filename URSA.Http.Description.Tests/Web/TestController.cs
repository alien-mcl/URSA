#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        /// <param name="page">Page of the collection. Use 0 for all of the entities.</param>
        /// <param name="pageSize">Page size. Ignored when <paramref name="page" /> is set to 0.</param>
        /// <returns>Collection of entities.</returns>
        public IEnumerable<Person> List(int page = 0, int pageSize = 0)
        {
            return new Person[0];
        }

        /// <summary>Gets the given person.</summary>
        /// <param name="id">Identifier of the person to retrieve.</param>
        /// <returns>Instance of the <see cref="Person" /> if matching <paramref name="id" />; otherwise <b>null</b>.</returns>
        public Person Get(Guid id)
        {
            return null;
        }

        /// <summary>Creates the specified person which <paramref name="id" /> is returned when created.</summary>
        /// <param name="id">The identifier.</param>
        /// <param name="person">The person.</param>
        /// <returns><b>true</b> if the person was created, <b>false</b> if failed; otherwise <b>null</b>.</returns>
        public bool? Create(out Guid id, Person person)
        {
            id = Guid.Empty;
            return null;
        }

        public bool? Update(Guid id, Person person)
        {
            return null;
        }

        public bool? Delete(Guid id)
        {
            return null;
        }
    }
}