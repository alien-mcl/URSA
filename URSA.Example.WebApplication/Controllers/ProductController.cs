using System;
using System.Collections.Generic;
using System.Linq;
using RomanticWeb;
using RomanticWeb.Entities;
using URSA.Example.WebApplication.Data;
using URSA.Web;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.Mapping;

namespace URSA.Example.WebApplication.Controllers
{
    /// <summary>Provides a basic product handling.</summary>
    public class ProductController : IWriteController<IProduct, Guid>
    {
        private readonly IList<IProduct> _repository;

        private readonly IEntityContext _entityContext;

        /// <summary>Initializes a new instance of the <see cref="ProductController"/> class.</summary>
        /// <param name="entityContext">The entity context.</param>
        public ProductController(IEntityContext entityContext)
        {
            if (entityContext == null)
            {
                throw new ArgumentNullException("entityContext");
            }

            _repository = new List<IProduct>();
            foreach (var product in (_entityContext = entityContext).AsQueryable<IProduct>())
            {
                _repository.Add(product);
            }
        }

        /// <inheritdoc />
        public IResponseInfo Response { get; set; }

        /// <summary>Gets all products.</summary>
        /// <param name="skip">Skips top <paramref name="skip" /> elements of the collection.</param>
        /// <param name="take">Takes top <paramref name="take" /> elements of the collection. Use 0 for all of the entities.</param>
        /// <returns>Collection of entities.</returns>
        public IEnumerable<IProduct> List([LinqServerBehavior(LinqOperations.Skip)] int skip = 0, [LinqServerBehavior(LinqOperations.Take)] int take = 0)
        {
            return _repository;
        }

        /// <summary>Gets the product with identifier of <paramref name="id" />.</summary>
        /// <param name="id">Identifier of the product to retrieve.</param>
        /// <returns>Instance of the <see cref="IProduct" /> if matching <paramref name="id" />; otherwise <b>null</b>.</returns>
        public IProduct Get(Guid id)
        {
            return (from entity in _repository where entity.Key == id select entity).FirstOrDefault();
        }

        /// <summary>Creates the specified product.</summary>
        /// <param name="product">The product.</param>
        /// <returns>Identifier of newly created product.</returns>
        public Guid Create(IProduct product)
        {
            product.Key = Guid.NewGuid();
            product.Context.Commit();
            product = product.Rename(new EntityId(product.Id.Uri + (product.Id.Uri.AbsoluteUri.EndsWith("/") ? String.Empty : "/") + "id/" + product.Key));
            _repository.Add(product);
            return product.Key;
        }

        /// <summary>Updates the specified product.</summary>
        /// <param name="id">The identifier of the product to be updated.</param>
        /// <param name="product">The product.</param>
        public void Update(Guid id, IProduct product)
        {
            (_repository[GetIndex(id)] = product).Key = id;
        }

        /// <summary>Deletes a product.</summary>
        /// <param name="id">Identifier of the product to be deleted.</param>
        public void Delete(Guid id)
        {
            var product = _repository[GetIndex(id)];
            _repository.Remove(product);
            _entityContext.Delete(product.Id);
            _entityContext.Commit();
        }

        private int GetIndex(Guid id)
        {
            int index = -1;
            _repository.Where((entity, entityIndex) => (entity.Key == id) && ((index = entityIndex) != -1)).FirstOrDefault();
            if (index == -1)
            {
                throw new NotFoundException(String.Format("Product with id of '{0}' does not exist.", id));
            }

            return index;
        }
    }
}