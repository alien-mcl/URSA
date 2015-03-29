using RomanticWeb.Model;
using System;
using System.Linq;
using RomanticWeb;
using RomanticWeb.Entities;

namespace URSA.Web.Http.Description.Model
{
    internal static class EntityQuadExtensions
    {
        internal static bool PredicateIs(this EntityQuad quad, IEntityContext context, Uri type)
        {
            if (!quad.Predicate.IsUri)
            {
                return false;
            }

            return context.Load<IEntity>(new EntityId(quad.Predicate.Uri)).GetTypes().Any(currentType => AbsoluteUriComparer.Default.Equals(currentType.Uri, type));
        }
    }
}