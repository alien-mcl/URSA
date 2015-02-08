using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System
{
    /// <summary>Provides useful <see cref="Type" /> extensions.</summary>
    public static class TypeExtensions
    {
        /// <summary>Checks if the type can be assigned to the <see cref="IEnumerable" /> interface.</summary> 
        /// <remarks>This method will return false for type <see cref="System.String" />.</remarks> 
        /// <param name="type">Type to be checked.</param> 
        /// <returns><b>true</b> if the type is <see cref="System.Array" /> or is assignable to <see cref="IEnumerable" /> (except <see cref="System.String" />); otherwise <b>false</b>.</returns> 
        public static bool IsEnumerable(this Type type) 
        { 
            return (type != null && ((type.IsArray) || ((typeof(IEnumerable).IsAssignableFrom(type)) && (type != typeof(string))))); 
        } 

        /// <summary>Gets the item type of the collection type or the type itself.</summary>
        /// <param name="type">Type for which find the item type.</param>
        /// <returns><see cref="Type" /> being the item of the collection or the <paramref name="type" /> itself.</returns>
        public static Type GetItemType(this Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            else if ((type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                return type.GetGenericArguments().First();
            }
            else if ((typeof(IEnumerable).IsAssignableFrom(type)) && (type != typeof(string)))
            {
                if (type.IsGenericType)
                {
                    return type.GetGenericArguments().First();
                }
                else
                {
                    return typeof(object);
                }
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

            if (values != null)
            {
                if (!targetType.IsEnumerable())
                {
                    return ((targetType.IsGenericType) && (targetType.GetGenericTypeDefinition() == typeof(Nullable<>)) ?
                        typeof(Nullable<>).MakeGenericType(itemType).GetConstructor(new[] { itemType }).Invoke(new[] { Convert.ChangeType(values.First(), itemType) }) :
                        Convert.ChangeType(values.First(), targetType));
                }
                else
                {
                    return CreateEnumerable(values, targetType, itemType ?? typeof(object));
                }
            }

            return null;
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