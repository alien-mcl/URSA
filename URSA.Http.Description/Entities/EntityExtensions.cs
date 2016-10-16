using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RomanticWeb;
using RomanticWeb.DotNetRDF;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Model;
using URSA.Web.Http.Description.Owl;
using URSA.Web.Http.Description.Rdfs;
using URSA.Web.Http.Description.Reflection;
using VDS.RDF;
using ICollection = URSA.Web.Http.Description.Hydra.ICollection;
using IResource = URSA.Web.Http.Description.Hydra.IResource;

namespace URSA.Web.Http.Description.Entities
{
    /// <summary>Provides useful <see cref="IEntity" /> extensions.</summary>
    public static class EntityExtensions
    {
        internal static readonly MethodInfo AsEntityMethod = typeof(RomanticWeb.Entities.EntityExtensions).GetMethod("AsEntity");

        /// <summary>Updates the specified <paramref name="target" /> entity with values from <paramref name="source" /> entity.</summary>
        /// <remarks>This method won't update nested entities' properties if the source values are <b>null</b>.</remarks>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="target">Target entity to be updated.</param>
        /// <param name="source">Source entity to get values from.</param>
        /// <returns>Updated <paramref name="target" /> entity.</returns>
        public static T Update<T>(this T target, T source) where T : class, IEntity
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return target.Update(source, 0);
        }

        /// <summary>Copies the specified entity to another context.</summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="entityContext">The target entity context to copy to.</param>
        /// <param name="entity">The entity to be copied.</param>
        /// <param name="newId">Optional new identifier.</param>
        /// <returns>Copy of the given <paramref name="entity" /> within given <paramref name="entityContext" />.</returns>
        public static T Copy<T>(this IEntityContext entityContext, T entity, EntityId newId = null) where T : class, IEntity
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (newId == null)
            {
                newId = entity.Id;
            }

            if ((entity.Context == entityContext) && (newId == entity.Id))
            {
                return entity;
            }

            return entityContext.Copy(entity, new KeyValuePair<EntityId, EntityId>(entity.Id, newId), new HashSet<EntityId>());
        }

        internal static bool IsCollection(this Rdfs.IClass @class)
        {
            return @class.IsClass(@class.Context.Mappings.MappingFor<ICollection>().Classes.First().Uri);
        }

        internal static IRestriction CreateRestriction(this IResource resource, Uri onProperty, IEntity allValuesFrom)
        {
            var typeConstrain = resource.Context.Create<IRestriction>(resource.CreateBlankId());
            typeConstrain.OnProperty = resource.Context.Create<IProperty>(new EntityId(onProperty));
            typeConstrain.AllValuesFrom = allValuesFrom;
            return typeConstrain;
        }

        internal static IRestriction CreateRestriction(this IResource resource, IProperty onProperty, uint maxCardinality)
        {
            var typeConstrain = resource.Context.Create<IRestriction>(resource.CreateBlankId());
            typeConstrain.OnProperty = onProperty;
            typeConstrain.MaxCardinality = maxCardinality;
            return typeConstrain;
        }

        internal static bool IsClass(this Rdfs.IClass @class, Uri type)
        {
            return ((!(@class.Id is BlankId)) && (AbsoluteUriComparer.Default.Equals(@class.Id.Uri, type))) ||
                (@class.SubClassOf.Any(superClass => superClass.IsClass(type)));
        }

        internal static IEnumerable<Rdfs.IClass> SuperClasses(this Rdfs.IClass @class)
        {
            IList<Rdfs.IClass> result = new List<Rdfs.IClass>();
            foreach (var superClass in @class.SubClassOf)
            {
                result.AddRange(superClass.SuperClasses());
                result.Add(superClass);
            }

            return result;
        }

        private static ITripleStore GetTripleStore(this IEntity entity)
        {
            var entitySourceField = entity.Context.GetType().GetField("_entitySource", BindingFlags.Instance | BindingFlags.NonPublic);
            if (entitySourceField == null)
            {
                throw new InvalidOperationException("Unknown type of entity context encountered.");
            }

            var entitySource = (IEntitySource)entitySourceField.GetValue(entity.Context);
            if (!(entitySource is TripleStoreAdapter))
            {
                throw new InvalidOperationException("Uknown entity source encountered.");
            }

            var store = (ITripleStore)entitySource.GetType().GetField("_store", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(entitySource);
            if ((!(store is TripleStore)) && (!(store is ThreadSafeTripleStore)))
            {
                throw new InvalidOperationException("Uknown triple store encountered.");
            }

            return store;
        }

        private static T Copy<T>(this IEntityContext entityContext, T entity, KeyValuePair<EntityId, EntityId> id, ISet<EntityId> visited) where T : class, IEntity
        {
            if (visited.Contains(id.Value))
            {
                return entityContext.Load<T>(id.Value);
            }

            if (visited.Contains(id.Key))
            {
                return entityContext.Load<T>(id.Value);
            }

            visited.Add(id.Value);
            var result = entityContext.Create<T>(id.Value);
            var typedResult = result.AsEntity<ITypedEntity>();
            foreach (var type in entity.GetTypes().Where(type => !AbsoluteUriComparer.Default.Equals(type.Uri, RomanticWeb.Vocabularies.Owl.Thing)))
            {
                typedResult.Types.Add(type);
            }

            foreach (var entityMapping in entityContext.Mappings)
            {
                if (!(entityMapping.EntityType.GetTypeInfo().IsInterface) ||
                    (!entityMapping.Classes.Join(typedResult.Types, outer => outer.Uri, inner => inner.Uri, (outer, inner) => outer, AbsoluteUriComparer.Default).Any()))
                {
                    continue;
                }

                var mappedResult = (IEntity)AsEntityMethod.MakeGenericMethod(entityMapping.EntityType).Invoke(null, new object[] { result });
                foreach (var property in entityMapping.Properties)
                {
                    var value = (object)entity.GetType().GetProperty(property.Name).GetValue(entity);
                    if (value == null)
                    {
                        continue;
                    }

                    if (property.ReturnType.GetTypeInfo().IsEnumerable())
                    {
                        value = entityContext.CopyCollection(mappedResult, property, value, id, visited);
                    }

                    if (value != null)
                    {
                        if ((property.ReturnType.GetTypeInfo().IsValueType) && (!entity.Context.Store.GetEntityQuads(entity.Id)
                            .Any(quad => (quad.Subject.ToEntityId().Equals(entity.Id)) &&
                                (AbsoluteUriComparer.Default.Equals(quad.Predicate.ToEntityId().Uri, property.Uri)))))
                        {
                            continue;
                        }

                        var entityValue = value as IEntity;
                        if (entityValue != null)
                        {
                            var newId = (entityValue.Id == id.Key ? id : new KeyValuePair<EntityId, EntityId>(id.Key, entityValue.Id));
                            value = AsEntityMethod.MakeGenericMethod(property.ReturnType).Invoke(null, new object[] { entityContext.Copy(entityValue, newId, visited) });
                        }

                        mappedResult.GetType().GetProperty(property.Name).SetValue(mappedResult, value);
                    }
                }
            }

            return result;
        }

        private static T Update<T>(this T target, T source, int depth) where T : class, IEntity
        {
            if (target == null)
            {
                return null;
            }

            if ((source == null) && (depth > 0))
            {
                return target;
            }

            if (target == source)
            {
                return target;
            }

            foreach (var property in target.Context.Mappings.MappingFor<T>().Properties)
            {
                var value = RomanticWeb.Entities.Proxies.DynamicExtensions.InvokeGet(source, property.Name);
                if (value == null)
                {
                    if (depth == 0)
                    {
                        RomanticWeb.Entities.Proxies.DynamicExtensions.InvokeSet(target, property.Name, null);
                    }

                    continue;
                }

                if (property.ReturnType.GetTypeInfo().IsEnumerable())
                {
                    value = target.UpdateCollection(property, (IEnumerable)value, depth);
                }
                else if (value is IEntity)
                {
                    var current = (IEntity)RomanticWeb.Entities.Proxies.DynamicExtensions.InvokeGet(target, property.Name);
                    value = (current != null ? current.Update((IEntity)value) :
                        AsEntityMethod.MakeGenericMethod(property.ReturnType).Invoke(null, new[] { target.Context.Copy((IEntity)value) })); 
                }

                if (value != null)
                {
                    RomanticWeb.Entities.Proxies.DynamicExtensions.InvokeSet(target, property.Name, value);
                }
            }

            return target;
        }

        private static object UpdateCollection<T>(this T target, IPropertyMapping property, IEnumerable source, int depth = 0) where T : class, IEntity
        {
            int indexOf = -1;
            IEnumerable current = (IEnumerable)RomanticWeb.Entities.Proxies.DynamicExtensions.InvokeGet(target, property.Name);
            var collection = (current != null ? new AbstractCollectionWrapper(current) : new AbstractCollectionWrapper(new List<object>()) { IsReplaced = true });
            foreach (var item in source)
            {
                var existing = collection
                    .Where((element, index) => (item is IEntity ? ((IEntity)item).Id == ((IEntity)element).Id : Equals(element, item)) && ((indexOf = index) != -1))
                    .FirstOrDefault();
                if (existing == null)
                {
                    collection.Add(item);
                }
                else if (item is IEntity)
                {
                    ((IEntity)existing).Update((IEntity)item, depth + 1);
                }
                else if (collection.IsList)
                {
                    collection[indexOf] = item;
                }
                else
                {
                    collection.Remove(existing);
                    collection.Add(item);
                }
            }

            var sourceCollection = new AbstractCollectionWrapper(source);
            var toBeRemoved = (from object item in current 
                               let existing = sourceCollection.Where((element, index) => (item is IEntity ? ((IEntity)item).Id == ((IEntity)element).Id : Equals(element, item)) && ((indexOf = index) != -1)).FirstOrDefault() 
                               where existing == null 
                               select item).ToList();
            foreach (var item in toBeRemoved)
            {
                collection.Remove(item);
            }

            return (collection.IsReplaced ? collection.Collection : null);
        }

        private static object CopyCollection<T>(this IEntityContext entityContext, T result, IPropertyMapping property, object value, KeyValuePair<EntityId, EntityId> id, ISet<EntityId> visited) where T : class, IEntity
        {
            var itemType = property.ReturnType.FindItemType();
            IEnumerable current = (IEnumerable)result.GetType().GetProperty(property.Name).GetValue(result);
            var collection = (current != null ? new AbstractCollectionWrapper(current) : new AbstractCollectionWrapper(new List<object>()) { IsReplaced = true });
            foreach (object item in (IEnumerable)value)
            {
                var itemToAdd = item;
                var entityValue = itemToAdd as IEntity;
                if (entityValue != null)
                {
                    var newId = (entityValue.Id == id.Key ? id : new KeyValuePair<EntityId, EntityId>(id.Key, entityValue.Id));
                    itemToAdd = AsEntityMethod.MakeGenericMethod(itemType).Invoke(null, new object[] { entityContext.Copy(entityValue, newId, visited) });
                }

                collection.Add(itemToAdd);
            }

            return (collection.IsReplaced ? collection.Collection : null);
        }
    }
}