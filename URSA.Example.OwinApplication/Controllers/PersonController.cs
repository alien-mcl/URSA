using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using URSA.Example.WebApplication.Data;
using URSA.Security;
using URSA.Web;
using URSA.Web.Http.Description.Mapping;
using URSA.Web.Mapping;

namespace URSA.Example.WebApplication.Controllers
{
    /// <summary>Provides a basic person handling.</summary>
    public class PersonController : IWriteController<Person, Guid>
    {
        private readonly IPersistingRepository<Person, Guid> _repository;

        /// <summary>Initializes a new instance of the <see cref="PersonController"/> class.</summary>
        /// <param name="repository">The repository.</param>
        public PersonController(IPersistingRepository<Person, Guid> repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            _repository = repository;
        }

        /// <inheritdoc />
        public IResponseInfo Response { get; set; }

        /// <summary>Gets all persons.</summary>
        /// <param name="totalItems">Number of total items in the collection if <paramref name="skip" /> and <paramref name="take" /> are used.</param>
        /// <param name="skip">Skips top <paramref name="skip" /> elements of the collection.</param>
        /// <param name="take">Takes top <paramref name="take" /> elements of the collection. Use 0 for all of the entities.</param>
        /// <param name="filter">Expression to be used for entity filtering.</param>
        /// <returns>Collection of entities.</returns>
        //// TODO: Take these parameter attributes to interface.
        public IEnumerable<Person> List(
            out int totalItems,
            [LinqServerBehavior(LinqOperations.Skip), FromQueryString("{?$skip}")] int skip = 0,
            [LinqServerBehavior(LinqOperations.Take), FromQueryString("{?$top}")] int take = 0,
            [LinqServerBehavior(LinqOperations.Filter), FromQueryString("{?$filter}")] Expression<Func<Person, bool>> filter = null)
        {
            totalItems = _repository.Count;
            return _repository.All(skip, take, (filter != null ? filter.Compile() : null));
        }

        /// <summary>Gets the person with identifier of <paramref name="id" />.</summary>
        /// <param name="id">Identifier of the person to retrieve.</param>
        /// <returns>Instance of the <see cref="Person" /> if matching <paramref name="id" />; otherwise <b>null</b>.</returns>
        public Person Get(Guid id)
        {
            return _repository.Get(id);
        }

        /// <summary>Creates the specified person.</summary>
        /// <param name="person">The person.</param>
        /// <returns>Identifier of newly created person.</returns>
        [DenyClaim(ClaimTypes.Anonymous)]
        public Guid Create(Person person)
        {
            person.Key = Guid.NewGuid();
            _repository.Create(person);
            return person.Key;
        }

        /// <summary>Updates the specified person.</summary>
        /// <param name="id">The identifier of the person to be updated.</param>
        /// <param name="person">The person.</param>
        [DenyClaim(ClaimTypes.Anonymous)]
        public void Update(Guid id, Person person)
        {
            person.Key = id;
            _repository.Update(person);
        }

        /// <summary>Deletes a person.</summary>
        /// <param name="id">Identifier of the person to be deleted.</param>
        [DenyClaim(ClaimTypes.Anonymous)]
        public void Delete(Guid id)
        {
            _repository.Delete(id);
        }

        /// <summary>Adds the given <paramref name="roles" /> to the user with given <paramref name="id" />.</summary>
        /// <param name="id">The identifier.</param>
        /// <param name="roles">The roles.</param>
        [DenyClaim(ClaimTypes.Anonymous)]
        public void SetRoles(Guid id, IEnumerable<string> roles)
        {
            var person = _repository.Get(id);
            if (person == null)
            {
                throw new NotFoundException(String.Format("Person with identifier of '{0}' does not exist.", id));
            }

            person.Roles.Clear();
            roles.ForEach(role => person.Roles.Add(role));
            _repository.Update(person);
        }

        /// <summary>Adds the given <paramref name="favouriteDishes" /> to the user with given <paramref name="id" />.</summary>
        /// <param name="id">The identifier.</param>
        /// <param name="favouriteDishes">Favourite dishes.</param>
        [DenyClaim(ClaimTypes.Anonymous)]
        public void SetFavouriteDishes(Guid id, IList<string> favouriteDishes)
        {
            var person = _repository.Get(id);
            if (person == null)
            {
                throw new NotFoundException(String.Format("Person with identifier of '{0}' does not exist.", id));
            }

            person.FavouriteDishes.Clear();
            favouriteDishes.ForEach(role => person.FavouriteDishes.Add(role));
            _repository.Update(person);
        }
    }
}