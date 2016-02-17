using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
            switch (instance.Key)
            {
                case 2:
                    throw new Exception("Unhandled server error.");
                case 3:
                    throw new AlreadyExistsException(String.Format("Person with identifier of '{0}' already exists.", instance.Key));
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
                    throw new NotFoundException(String.Format("Person with identifier of '{0}' does not exist.", instance.Key));
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

            return new Person() { Key = id };
        }

        public IEnumerable<Person> List(out int totalItems, int skip = 0, int take = 0, Expression<Func<Person, bool>> filter = null)
        {
            totalItems = 1;
            return new[] { new Person() { Key = 1 } };
        }

        public void SetRoles(int id, IEnumerable<string> roles)
        {
        }
    }
}