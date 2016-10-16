using RomanticWeb.Model;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RomanticWeb;
using RomanticWeb.Entities;
using URSA.Web.Http.Converters;

namespace URSA.Web.Http.Description.Model
{
    internal static class EntityQuadExtensions
    {
        [ExcludeFromCodeCoverage]
        internal static bool PredicateIs(this IEntityQuad quad, IEntityContext context, Uri type)
        {
            return (quad.Predicate.IsUri) && (!quad.Predicate.Uri.AbsoluteUri.StartsWith(EntityConverter.Hydra.AbsoluteUri)) && 
                (context.Load<IEntity>(new EntityId(quad.Predicate.Uri)).GetTypes().Any(currentType => AbsoluteUriComparer.Default.Equals(currentType.Uri, type)));
        }
    }
}