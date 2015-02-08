using System;
using System.Collections.Generic;
using URSA.Http.Description.Tests.Data;
using URSA.Web.Mapping;

namespace URSA.Web.Http.Description.Tests
{
    [Route("api/person")]
    public class TestController : IController<Person>, IReadController<Person, Guid>, IWriteController<Person, Guid>
    {
        public IResponseInfo Response { get; set; }

        public IEnumerable<Person> Get(int page = 0, int pageSize = 0)
        {
            return new Person[0];
        }

        public Person Get(Guid id)
        {
            return null;
        }

        public bool? Create(Person person, out Guid id)
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