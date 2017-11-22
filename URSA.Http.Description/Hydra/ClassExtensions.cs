using System.Collections.Generic;
using System.Linq;
using RDeF.Entities;
using URSA.Web.Http.Description.Model;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Provides useful <see cref="IClass" /> extensions.</summary>
    public static class ClassExtensions
    {
        /// <summary>Retrieves all operations of a given <paramref name="class" /> with their corresponding <see cref="IIriTemplate" /> if any.</summary>
        /// <param name="class">The class for which to retrieve operations.</param>
        /// <returns>Enumeration of key-value pairs of <see cref="IOperation" /> and it's corresponding <see cref="IIriTemplate" /> or <b>null</b> if there is none.</returns>
        public static IEnumerable<LinkedOperation> GetSupportedOperations(this IClass @class)
        {
            return (from operation in @class.SupportedOperations
                    select new LinkedOperation(operation, null))
                .Union(
                    from quad in ((ISerializableEntitySource)@class.Context.EntitySource).Statements
                    where (quad.Object != null) && (quad.Subject == @class.Iri) &&
                        (quad.PredicateIs(@class.Context, @class.Context.Mappings.FindEntityMappingFor<ITemplatedLink>(null).Classes.First().Term))
                    let templatedLink = @class.Context.Load<ITemplatedLink>(quad.Predicate)
                    from operation in templatedLink.SupportedOperations
                    select new LinkedOperation(operation, @class.Context.Load<IIriTemplate>(quad.Object)));
        }
    }
}