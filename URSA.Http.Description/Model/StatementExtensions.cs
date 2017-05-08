using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RDeF.Entities;
using URSA.Web.Http.Converters;

namespace URSA.Web.Http.Description.Model
{
    internal static class StatementExtensions
    {
        [ExcludeFromCodeCoverage]
        internal static bool PredicateIs(this Statement statement, IEntityContext context, Iri type)
        {
            return (!statement.Predicate.ToString().StartsWith(EntityConverter.Hydra.AbsoluteUri)) && (context.Load<IEntity>(statement.Predicate).Is(type));
        }
    }
}