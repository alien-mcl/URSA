using System;
using System.Collections.Generic;
using URSA.Web.Http.Tests.Data;
using URSA.Web.Mapping;

namespace URSA.Web.Tests
{
    [Route("api/test")]
    public class CrudController : IWriteController<Person, int>
    {
        public IResponseInfo Response { get; set; }

        public bool? Create(out int id, Person instance)
        {
            switch (instance.Id)
            {
                case 2:
                    id = 0;
                    return null;
                case 3:
                    id = 0;
                    return false;
            }

            id = 1;
            return true;
        }

        public bool? Update(int id, Person instance)
        {
            switch (id)
            {
                case 2:
                    return null;
                case 3:
                    return false;
            }

            return true;
        }

        public bool? Delete(int id)
        {
            switch (id)
            {
                case 2:
                    return null;
                case 3:
                    return false;
            }

            return true;
        }

        public Person Get(int id)
        {
            switch (id)
            {
                case 2:
                    return null;
            }

            return new Person() { Id = id };
        }

        public IEnumerable<Person> List(int skip = 0, int take = 0)
        {
            return new Person[] { new Person() { Id = 1 } };
        }
    }
}