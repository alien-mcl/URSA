using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace URSA.Web.Http.Description.Reflection
{
    internal class AbstractCollectionWrapper : IList<object>
    {
        private readonly IEnumerable _collection;
        private readonly Action<object, int> _setAt;
        private readonly Func<int, object> _getAt;
        private readonly Action<object> _add;
        private readonly Func<object, bool> _remove;
        private readonly Func<object, bool> _contains;
        private readonly Func<object, int> _indexOf;
        private readonly bool _isList;

        internal AbstractCollectionWrapper(IEnumerable collection)
        {
            _collection = collection;
            var collectionType = collection.GetType();
            bool isCollectionInitialized = false;
            foreach (var @interface in collectionType.GetInterfaces())
            {
                if (@interface.GetTypeInfo().IsGenericType)
                {
                    var genericType = @interface.GetGenericTypeDefinition();
                    if (genericType == typeof(IList<>))
                    {
                        _isList = isCollectionInitialized = Initialize(collectionType, @interface, out _add, out _remove, out _contains, true, out _setAt, out _getAt, out _indexOf);
                        break;
                    }

                    if (genericType == typeof(ICollection<>))
                    {
                        isCollectionInitialized = Initialize(collectionType, @interface, out _add, out _remove, out _contains, false, out _setAt, out _getAt, out _indexOf);
                    }
                }
                else if ((@interface == typeof(IList)) && (!_isList))
                {
                    _isList = isCollectionInitialized = Initialize(collectionType, @interface, out _add, out _remove, out _contains, true, out _setAt, out _getAt, out _indexOf);
                }
                else if ((@interface == typeof(ICollection)) && (!isCollectionInitialized))
                {
                    isCollectionInitialized = Initialize(collectionType, @interface, out _add, out _remove, out _contains, true, out _setAt, out _getAt, out _indexOf);
                }
            }

            if ((!_isList) && (!isCollectionInitialized))
            {
                var replacement = new List<object>(collection.Cast<object>());
                foreach (var item in collection)
                {
                    replacement.Add(item);
                }

                _add = item => replacement.Add(item);
                _remove = item => replacement.Remove(item);
                _getAt = index => replacement[index];
                _setAt = (item, index) => replacement[index] = item;
                _contains = item => replacement.Contains(item);
                _indexOf = item => replacement.IndexOf(item);
                _collection = replacement;
                IsReplaced = true;
            }
        }

        /// <inheritdoc />
        int ICollection<object>.Count { get { throw new NotImplementedException(); } }

        /// <inheritdoc />
        bool ICollection<object>.IsReadOnly { get { throw new NotImplementedException(); } }

        internal bool IsList { get { return _isList; } }

        internal bool IsReplaced { get; set; }

        internal IEnumerable Collection { get { return _collection; } }

        /// <inheritdoc />
        public object this[int index] { get { return _getAt(index); } set { _setAt(value, index); } }

        /// <inheritdoc />
        public int IndexOf(object item)
        {
            return _indexOf(item);
        }

        /// <inheritdoc />
        public void Add(object item)
        {
            _add(item);
        }

        /// <inheritdoc />
        public bool Remove(object item)
        {
            return _remove(item);
        }

        /// <inheritdoc />
        public IEnumerator<object> GetEnumerator()
        {
            return _collection.Cast<object>().GetEnumerator();
        }

        /// <inheritdoc />
        public bool Contains(object item)
        {
            return _contains(item);
        }

        /// <inheritdoc />
        void IList<object>.Insert(int index, object item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        void IList<object>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        void ICollection<object>.Clear()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        void ICollection<object>.CopyTo(object[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        private bool Initialize(
            Type collectionType,
            Type @interface,
            out Action<object> add,
            out Func<object, bool> remove,
            out Func<object, bool> contains,
            bool includeList,
            out Action<object, int> setAt,
            out Func<int, object> getAt,
            out Func<object, int> indexOf)
        {
            MethodInfo indexer = null;
            MethodInfo getter = null;
            MethodInfo setter = null;
            MethodInfo adder = null;
            MethodInfo remover = null;
            MethodInfo checker = null;
            var methods = from item in new[] { @interface }.Union(@interface.GetInterfaces())
                          from method in collectionType.GetTypeInfo().GetRuntimeInterfaceMap(item).TargetMethods
                          select method;
            foreach (var method in methods)
            {
                switch (method.Name)
                {
                    case "get_Item":
                        getter = method;
                        break;
                    case "set_Item":
                        setter = method;
                        break;
                    case "IndexOf":
                        indexer = method;
                        break;
                    case "Add":
                        adder = method;
                        break;
                    case "Remove":
                        remover = method;
                        break;
                    case "Contains":
                        checker = method;
                        break;
                }

                if ((getter != null) && (setter != null) && (checker != null) && ((!includeList) || ((adder != null) && (remover != null) && (indexer != null))))
                {
                    break;
                }
            }

            add = item => adder.Invoke(_collection, new[] { item });
            remove = item => (bool)remover.Invoke(_collection, new[] { item });
            contains = item => (bool)checker.Invoke(_collection, new[] { item });
            if (includeList)
            {
                setAt = (item, index) => setter.Invoke(_collection, new[] { index, item });
                getAt = index => getter.Invoke(_collection, new object[] { index });
                indexOf = item => (int)indexer.Invoke(_collection, new[] { item });
            }
            else
            {
                setAt = null;
                getAt = null;
                indexOf = null;
            }

            return true;
        }
    }
}