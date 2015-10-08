using System.Collections.Generic;
using RomanticWeb.Entities;
using URSA.Example.WebApplication.Data;
using URSA.Web;
using URSA.Web.Http.Description.Mapping;

namespace URSA.Example.WebApplication.Controllers
{
    /// <summary>Provides a basic product handling.</summary>
    public class ProductController : IWriteController<IProduct, EntityId>
    {
        /// <inheritdoc />
        public IResponseInfo Response { get; set; }

        /// <summary>Gets all products.</summary>
        /// <param name="skip">Skips top <paramref name="skip" /> elements of the collection.</param>
        /// <param name="take">Takes top <paramref name="take" /> elements of the collection. Use 0 for all of the entities.</param>
        /// <returns>Collection of entities.</returns>
        public IEnumerable<IProduct> List([LinqServerBehavior(LinqOperations.Skip)] int skip = 0, [LinqServerBehavior(LinqOperations.Take)] int take = 0)
        {
            return new IProduct[0];
        }

        /// <summary>Gets the product with identifier of <paramref name="id" />.</summary>
        /// <param name="id">Identifier of the product to retrieve.</param>
        /// <returns>Instance of the <see cref="IProduct" /> if matching <paramref name="id" />; otherwise <b>null</b>.</returns>
        public IProduct Get(EntityId id)
        {
            return null;
        }

        /// <summary>Creates the specified product.</summary>
        /// <param name="product">The product.</param>
        /// <returns>Identifier of newly created product.</returns>
        public EntityId Create(IProduct product)
        {
            return new EntityId("http://temp.uri/product");
        }

        /// <summary>Updates the specified product.</summary>
        /// <param name="id">The identifier of the product to be updated.</param>
        /// <param name="product">The product.</param>
        public void Update(EntityId id, IProduct product)
        {
        }

        /// <summary>Deletes a product.</summary>
        /// <param name="id">Identifier of the product to be deleted.</param>
        public void Delete(EntityId id)
        {
        }
    }
}