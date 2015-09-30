using System;
using System.Collections.Generic;
using URSA.Web.Http.Tests.Data;
using URSA.Web.Mapping;

namespace URSA.Web.Tests
{
    [Route("api/person")]
    public class CrudController : IWriteController<Person, int>
    {
        public IResponseInfo Response { get; set; }

        public int Create(Person instance)
        {
            switch (instance.Id)
            {
                case 2:
                    throw new Exception("Unhandled server error.");
                case 3:
                    throw new AlreadyExistsException(String.Format("Person with identifier of '{0}' already exists.", instance.Id));
            }

            return 1;
        }

        public void Update(int id, Person instance)
        {
            switch (id)
            {
                case 2:
                    throw new Exception("Unhandled server error.");
                case 3:
                    throw new NotFoundException(String.Format("Person with identifier of '{0}' does not exist.", instance.Id));
            }
        }

        public void Delete(int id)
        {
            switch (id)
            {
                case 2:
                    throw new Exception("Unhandled server error.");
                case 3:
                    throw new NotFoundException(String.Format("Person with identifier of '{0}' does not exist.", id));
            }
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

        public void SetRoles(int id, IEnumerable<string> roles)
        {
        }
    }
}