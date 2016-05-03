using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace URSA.Web.Http
{
    /// <summary>Provides an easy access to key-value query string parameters.</summary>
    public class ParametersCollection : IEnumerable<KeyValuePair<string, string>>
    {
        private static readonly string[] NoValues = new string[0];

        private readonly string _separator;
        private readonly string _valueIndicator;
        private readonly IDictionary<string, ISet<string>> _container;

        internal ParametersCollection(string separator, string valueIndicator)
        {
            _separator = separator;
            _valueIndicator = valueIndicator;
            _container = new ConcurrentDictionary<string, ISet<string>>();
        }

        /// <summary>Gets the number of keys.</summary>
        public int Count { get { return _container.Count; } }

        /// <summary>Gets or sets a value for a given <paramref name="key" />.</summary>
        /// <remarks>If the <paramref name="key" /> is null or empty and the value being set is not, the value will be used as a key, and an empty string will be set.</remarks>
        /// <param name="key">The key.</param>
        /// <returns>Single value for the given <paramref name="key" /> or <b>null</b> if it doesn't exist.</returns>
        public string this[string key]
        {
            get
            {
                ISet<string> result;
                if ((String.IsNullOrEmpty(key)) || (!_container.TryGetValue(key, out result)))
                {
                    return null;
                }

                return result.First();
            }

            internal set
            {
                if ((String.IsNullOrEmpty(key)) && (String.IsNullOrEmpty(value)))
                {
                    return;
                }

                if ((String.IsNullOrEmpty(key)) && (!String.IsNullOrEmpty(value)))
                {
                    key = value;
                    value = String.Empty;
                }

                (_container[key] = new HashSet<string>()).Add(value ?? String.Empty);
            }
        }

        /// <summary>Gets the values for a given <paramref name="key" />.</summary>
        /// <param name="key">The key.</param>
        /// <returns>Enumeration of strings being values for a given <paramref name="key" />.</returns>
        public IEnumerable<string> GetValues(string key)
        {
            ISet<string> result;
            if ((String.IsNullOrEmpty(key)) || (!_container.TryGetValue(key, out result)))
            {
                return NoValues;
            }

            return result;
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return new ParametersCollectionEnumerator(_container);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ToStringInternal();
        }

        /// <summary>Returns a string representation of this parameters collection escaping to keep chars in the <paramref name="allowedChars" />.</summary>
        /// <param name="allowedChars">Allowed chars.</param>
        /// <returns>String representation of this parameters collection.</returns>
        public string ToString(char[] allowedChars)
        {
            if (allowedChars == null)
            {
                throw new ArgumentNullException("allowedChars");
            }

            if (allowedChars.Length == 0)
            {
                throw new ArgumentOutOfRangeException("allowedChars");
            }

            return ToStringInternal(allowedChars);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>Adds a new key-value pair.</summary>
        /// <remarks>If the given <paramref name="key" /> is either null or empty and the <paramref name="value" /> is not, <paramref name="value" /> will be used as a key, and an empty string will be added.</remarks>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        internal void AddValue(string key, string value)
        {
            if ((String.IsNullOrEmpty(key)) && (String.IsNullOrEmpty(value)))
            {
                return;
            }

            if ((String.IsNullOrEmpty(key)) && (!String.IsNullOrEmpty(value)))
            {
                key = value;
                value = String.Empty;
            }

            ISet<string> values;
            if (!_container.TryGetValue(key, out values))
            {
                _container[key] = values = new HashSet<string>();
            }

            values.Add(value ?? String.Empty);
        }

        private string ToStringInternal(char[] allowedChars = null)
        {
            StringBuilder result = new StringBuilder(256);
            foreach (var entry in _container)
            {
                foreach (var value in entry.Value)
                {
                    result.AppendFormat(
                        "{0}{1}{2}",
                        _separator,
                        (allowedChars != null ? UrlParser.ToSafeString(entry.Key, allowedChars) : entry.Key),
                        (!String.IsNullOrEmpty(value) ? String.Format("{0}{1}", _valueIndicator, (allowedChars != null ? UrlParser.ToSafeString(value, allowedChars) : value)) : String.Empty));
                }
            }

            return (result.Length > 0 ? result.ToString(1, result.Length - 1) : String.Empty);
        }

        private class ParametersCollectionEnumerator : IEnumerator<KeyValuePair<string, string>>
        {
            private readonly IEnumerator<KeyValuePair<string, ISet<string>>> _keyEnumerator;
            private IEnumerator<string> _valueEnumerator = null;
            private bool _disposed;

            internal ParametersCollectionEnumerator(IDictionary<string, ISet<string>> container)
            {
                _keyEnumerator = container.GetEnumerator();
            }

            /// <inheritdoc />
            public KeyValuePair<string, string> Current { get; private set; }

            /// <inheritdoc />
            object IEnumerator.Current { get { return Current; } }

            /// <inheritdoc />
            public void Dispose()
            {
                _disposed = true;
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (_disposed)
                {
                    throw new InvalidOperationException("Enumerator has been already disposed.");
                }

                if ((_valueEnumerator == null) || (!_valueEnumerator.MoveNext()))
                {
                    return MoveNextKey();
                }

                Current = new KeyValuePair<string, string>(_keyEnumerator.Current.Key, _valueEnumerator.Current);
                return true;
            }

            /// <inheritdoc />
            public void Reset()
            {
                if (_disposed)
                {
                    throw new InvalidOperationException("Enumerator has been already disposed.");
                }

                _keyEnumerator.Reset();
                _valueEnumerator = null;
            }

            private bool MoveNextKey()
            {
                if ((!_keyEnumerator.MoveNext()) || (!(_valueEnumerator = _keyEnumerator.Current.Value.GetEnumerator()).MoveNext()))
                {
                    return false;
                }

                Current = new KeyValuePair<string, string>(_keyEnumerator.Current.Key, _valueEnumerator.Current);
                return true;
            }
        }
    }
}