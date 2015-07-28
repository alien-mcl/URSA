using System;
using System.Linq;
using RomanticWeb.Entities;
using RomanticWeb.Model;

namespace URSA.Web.Http.Description.Entities
{
    /// <summary>Provides useful <see cref="IEntity" /> extensions.</summary>
    public static class EntityExtensions
    {
        /// <summary>Clones a given blank node.</summary>
        /// <remarks>This method expect <paramref name="source" /> to be a blank node. If it's not the case, original instance will be returned.</remarks>
        /// <typeparam name="T">Type of the entity being cloned.</typeparam>
        /// <param name="source">The source entity.</param>
        /// <returns>New copy of the given <paramref name="source" />.</returns>
        public static T Clone<T>(this T source) where T : class, IEntity
        {
            if (!(source.Id is BlankId))
            {
                return source;
            }

            var blankId = (BlankId)source.Id;
            if (blankId.RootEntityId == null)
            {
                throw new InvalidOperationException("No root entity was found.");
            }

            var newBlankId = source.Context.Load<IEntity>(blankId.RootEntityId).CreateBlankId();
            var result = source.Context.Create<T>(newBlankId);
            var quads = from quad in source.Context.Store.Quads
                        where (quad.Subject.IsBlank) && (quad.Subject.ToEntityId() == blankId)
                        let subject = Node.ForBlank(newBlankId.Identifier, newBlankId.RootEntityId, newBlankId.Graph)
                        select new EntityQuad(newBlankId, subject, quad.Predicate, quad.Object, quad.Graph);
            result.Context.Store.AssertEntity(result.Id, quads);
            return result;
        }
    }
}