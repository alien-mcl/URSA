using RomanticWeb.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RomanticWeb;
using RomanticWeb.Entities;
using RomanticWeb.Vocabularies;

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

            var id = new EntityId(quad.Predicate.Uri);
            context.Load<IEntity>(id);
            return (from item in context.Store.GetEntityQuads(id)
                    where (item.Predicate.IsUri) && (AbsoluteUriComparer.Default.Equals(item.Predicate.Uri, Rdf.type)) &&
                          (item.Object.IsUri) && (AbsoluteUriComparer.Default.Equals(item.Object.Uri, type))
                    select item).Any();
        }
    }
}