using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Reflection
{
    /// <summary>Provides useful <see cref="Type" /> extensions.</summary>
    public static class TypeExtensions
    {
        /// <summary>Checks if a given type is of either type <see cref="IList" /> or <see cref="IList{T}"/>.</summary>
        /// <param name="type">Type to check.</param>
        /// <returns><b>true</b> if the type implements a proper interface; otherwise <b>false</b>.</returns>
        public static bool IsList(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return (!type.IsArray) && ((type.GetInterfaces().Any(@interface => (typeof(IList).IsAssignableFrom(@interface)))) || (type.IsGenericList()));
        }

        /// <summary>Checks if a given type is of type <see cref="IList{T}" />.</summary>
        /// <param name="type">Type to check.</param>
        /// <returns><b>true</b> if the type implements a proper interface; otherwise <b>false</b>.</returns>
        public static bool IsGenericList(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return (!type.IsArray) && (((type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(IList<>))) ||
                (type.GetInterfaces().Any(@interface => (@interface.IsGenericTypeDefinition) && (typeof(IList<>).IsAssignableFrom(@interface.GetGenericTypeDefinition())))));
        }

        /// <summary>Checks if a given type is of either type <see cref="ICollection" /> or <see cref="ICollection{T}" />.</summary>
        /// <param name="type">Type to check.</param>
        /// <returns><b>true</b> if the type implements a proper interface; otherwise <b>false</b>.</returns>
        public static bool IsCollection(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return (!type.IsArray) && ((type.GetInterfaces().Any(@interface => (typeof(ICollection).IsAssignableFrom(@interface)))) || (type.IsGenericCollection()));
        }

        /// <summary>Checks if a given type is of type <see cref="ICollection{T}" />.</summary>
        /// <param name="type">Type to check.</param>
        /// <returns><b>true</b> if the type implements a proper interface; otherwise <b>false</b>.</returns>
        public static bool IsGenericCollection(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return (!type.IsArray) && (((type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(ICollection<>))) ||
                (type.GetInterfaces().Any(@interface => (@interface.IsGenericTypeDefinition) && (typeof(ICollection<>).IsAssignableFrom(@interface.GetGenericTypeDefinition())))));
        }

        /// <summary>Checks if the type can be assigned to the <see cref="IEnumerable" /> interface.</summary> 
        /// <remarks>This method will return false for type <see cref="System.String" />.</remarks> 
        /// <param name="type">Type to be checked.</param> 
        /// <returns><b>true</b> if the type is <see cref="System.Array" /> or is assignable to <see cref="IEnumerable" /> (except <see cref="System.String" />); otherwise <b>false</b>.</returns> 
        public static bool IsEnumerable(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return ((type.IsArray) || ((typeof(IEnumerable).IsAssignableFrom(type)) && (type != typeof(string)))) || (type.IsGenericEnumerable());
        } 

        /// <summary>Checks if a given type is of type <see cref="IEnumerable{T}" />.</summary>
        /// <param name="type">Type to check.</param>
        /// <returns><b>true</b> if the type implements a proper interface; otherwise <b>false</b>.</returns>
        public static bool IsGenericEnumerable(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return (!type.IsArray) && (((type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(IEnumerable<>))) ||
                (type.GetInterfaces().Any(@interface => (@interface.IsGenericTypeDefinition) && (typeof(IEnumerable<>).IsAssignableFrom(@interface.GetGenericTypeDefinition())))));
        }

        /// <summary>Gets the item type of the collection type or the type itself.</summary>
        /// <param name="type">Type for which find the item type.</param>
        /// <returns><see cref="Type" /> being the item of the collection or the <paramref name="type" /> itself.</returns>
        public static Type GetItemType(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (type.IsArray)
            {
                return type.GetElementType();
            }
            
            if ((type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                return type.GetGenericArguments().First();
            }
            
            if ((typeof(IEnumerable).IsAssignableFrom(type)) && (type != typeof(string)))
            {
                return (type.IsGenericType ? type.GetGenericArguments().First() : typeof(object));
            }

            return type;
        }

        /// <summary>Creates an instance of the <paramref name="targetType" /> from an enumeration of objects.</summary>
        /// <param name="values">Objects to feed the new instance.</param>
        /// <param name="targetType">Target type of the instance.</param>
        /// <param name="itemType">Optional collection item type.</param>
        /// <returns>Instance of the <paramref name="targetType" />.</returns>
        public static object MakeInstance(this IEnumerable<object> values, Type targetType, Type itemType = null)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            if (values == null)
            {
                return null;
            }

            if (!targetType.IsEnumerable())
            {
                return ((targetType.IsGenericType) && (targetType.GetGenericTypeDefinition() == typeof(Nullable<>)) ?
                    typeof(Nullable<>).MakeGenericType(itemType).GetConstructor(new[] { itemType }).Invoke(new[] { Convert.ChangeType(values.First(), itemType) }) :
                    Convert.ChangeType(values.First(), targetType));
            }

            return CreateEnumerable(values, targetType, itemType ?? typeof(object));
        }

        private static IEnumerable CreateEnumerable(IEnumerable<object> values, Type type, Type itemType)
        {
            if (values.GetType() == type)
            {
                return values;
            }

            if (type.IsArray)
            {
                int count = values.Count();
                Array result = Array.CreateInstance(itemType, count);
                int index = 0;
                foreach (var item in values)
                {
                    result.SetValue(Convert.ChangeType(item, itemType), index);
                    index++;
                }

                return result;
            }
            else
            {
                IList result = (IList)typeof(IList<>).MakeGenericType(itemType).GetConstructor(new Type[0]).Invoke(null);
                foreach (var item in values)
                {
                    result.Add(Convert.ChangeType(item, itemType));
                }

                return result;
            }
        }
    }
}