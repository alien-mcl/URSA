using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using RDeF.Entities;
using URSA.Example.WebApplication.Data;
using URSA.Web;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.Mapping;
using URSA.Web.Mapping;

namespace URSA.Example.WebApplication.Controllers
{
    /// <summary>Provides a basic product handling.</summary>
    public class ProductController : IWriteController<IProduct, Guid>
    {
        private readonly IEntityContext _entityContext;

        /// <summary>Initializes a new instance of the <see cref="ProductController"/> class.</summary>
        /// <param name="entityContext">The entity context.</param>
        public ProductController(IEntityContext entityContext)
        {
            if (entityContext == null)
            {
                throw new ArgumentNullException("entityContext");
            }

            _entityContext = entityContext;
        }

        /// <inheritdoc />
        public IResponseInfo Response { get; set; }

        /// <summary>Gets all products.</summary>
        /// <param name="totalItems">Number of total items in the collection if <paramref name="skip" /> and <paramref name="take" /> are used.</param>
        /// <param name="skip">Skips top <paramref name="skip" /> elements of the collection.</param>
        /// <param name="take">Takes top <paramref name="take" /> elements of the collection. Use 0 for all of the entities.</param>
        /// <param name="filter">Expression to be used for entity filtering.</param>
        /// <returns>Collection of entities.</returns>
        public IEnumerable<IProduct> List(
            out int totalItems,
            [LinqServerBehavior(LinqOperations.Skip), FromQueryString("{?$skip}")] int skip = 0,
            [LinqServerBehavior(LinqOperations.Take), FromQueryString("{?$top}")] int take = 0,
            [LinqServerBehavior(LinqOperations.Filter), FromQueryString("{?$filter}")] Expression<Func<IProduct, bool>> filter = null)
        {
            totalItems = _entityContext.AsQueryable<IProduct>().ToList().Count();
            IEnumerable<IProduct> result = _entityContext.AsQueryable<IProduct>();
            if (skip > 0)
            {
                result = result.Skip(skip);
            }

            if (take > 0)
            {
                result = result.Take(take);
            }

            if (filter != null)
            {
                result = result.Where(entity => filter.Compile()(entity));
            }

            return result;
        }

        /// <summary>Gets the product with identifier of <paramref name="id" />.</summary>
        /// <param name="id">Identifier of the product to retrieve.</param>
        /// <returns>Instance of the <see cref="IProduct" /> if matching <paramref name="id" />; otherwise <b>null</b>.</returns>
        public IProduct Get(Guid id)
        {
            return (from entity in _entityContext.AsQueryable<IProduct>() where entity.Key == id select entity).FirstOrDefault();
        }

        /// <summary>Creates the specified product.</summary>
        /// <param name="product">The product.</param>
        /// <returns>Identifier of newly created product.</returns>
        public Guid Create(IProduct product)
        {
            Guid id = Guid.NewGuid();
            product = _entityContext.Copy(product, new Iri(product.Iri + (product.Iri.ToString().EndsWith("/") ? String.Empty : "/") + id));
            product.Key = id;
            _entityContext.Commit();
            return product.Key;
        }

        /// <summary>Updates the specified product.</summary>
        /// <param name="id">The identifier of the product to be updated.</param>
        /// <param name="product">The product.</param>
        public void Update(Guid id, IProduct product)
        {
            GetInternal(id).Update(product);
            _entityContext.Commit();
        }

        /// <summary>Deletes a product.</summary>
        /// <param name="id">Identifier of the product to be deleted.</param>
        public void Delete(Guid id)
        {
            var product = GetInternal(id);
            _entityContext.Delete(product.Iri);
            _entityContext.Commit();
        }

        private IProduct GetInternal(Guid id)
        {
            var result = (from entity in _entityContext.AsQueryable<IProduct>() where entity.Key == id select entity).FirstOrDefault();
            if (result == null)
            {
                throw new NotFoundException(String.Format("Product with id of '{0}' does not exist.", id));
            }

            return result;
        }
    }
}