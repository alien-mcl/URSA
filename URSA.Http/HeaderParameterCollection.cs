using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace URSA.Web.Http
{
    /// <summary>Represents a collection of <see cref="HeaderParameter" />.</summary>
    public sealed class HeaderParameterCollection : ICollection<HeaderParameter>
    {
        private static readonly IEqualityComparer<string> Comparer = StringComparer.OrdinalIgnoreCase;
        private readonly IDictionary<string, HeaderParameter> _parameters = new Dictionary<string, HeaderParameter>(Comparer);

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public int Count { get { return _parameters.Count; } }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        bool ICollection<HeaderParameter>.IsReadOnly { get { return _parameters.IsReadOnly; } }

        /// <summary>Gets or sets the parameter by it's name.</summary>
        /// <param name="parameter">Name of the parameter.</param>
        /// <returns>Instance of the <see cref="HeaderParameter" /> if the parameter of <paramref name="parameter" /> exists; otherwise <b>null</b>.</returns>
        public HeaderParameter this[string parameter]
        {
            get
            {
                if (parameter == null)
                {
                    throw new ArgumentNullException("parameter");
                }

                HeaderParameter result;
                if (!_parameters.TryGetValue(parameter, out result))
                {
                    result = null;
                }

                return result;
            }

            set
            {
                if (parameter == null)
                {
                    throw new ArgumentNullException("parameter");
                }

                if (parameter.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("parameter");
                }

                if ((value != null) && (!Comparer.Equals(parameter, value.Name)))
                {
                    throw new InvalidOperationException(String.Format("Parameter name '{0}' and actual parameter '{1}' mismatch.", parameter, value.Name));
                }

                Add(value);
            }
        }

        /// <summary>Adds the parameter to the collection.</summary>
        /// <remarks>If the parameter already exists, it's value is updated with the new one.</remarks>
        /// <param name="parameter">Parameter to be added.</param>
        public void Add(HeaderParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            HeaderParameter current;
            if (!_parameters.TryGetValue(parameter.Name, out current))
            {
                _parameters[parameter.Name] = parameter;
            }
            else
            {
                current.Value = parameter.Value;
            }
        }

        /// <summary>Adds the parameter to the collection.</summary>
        /// <remarks>If the parameter already exists, it's value is updated with the <b>null</b> value.</remarks>
        /// <param name="parameter">Parameter to be added.</param>
        public void Add(string parameter)
        {
            Add(parameter, null);
        }

        /// <summary>Adds the parameter to the collection.</summary>
        /// <remarks>If the parameter already exists, it's value is updated with the new one.</remarks>
        /// <param name="parameter">Parameter to be added.</param>
        /// <param name="value">Value of the parameter.</param>
        public void Add(string parameter, object value)
        {
            Add(new HeaderParameter(parameter, value));
        }

        /// <summary>Removes a parameter.</summary>
        /// <param name="parameter">Parameter to be removed.</param>
        /// <returns><b>true</b> if the parameter was removed;, otherwise <b>false</b>.</returns>
        public bool Remove(string parameter)
        {
            return _parameters.Remove(parameter);
        }

        /// <summary>Removes a parameter.</summary>
        /// <param name="parameter">Parameter to be removed.</param>
        /// <returns><b>true</b> if the parameter was removed;, otherwise <b>false</b>.</returns>
        public bool Remove(HeaderParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            HeaderParameter result;
            if ((_parameters.TryGetValue(parameter.Name, out result)) && (((result.Value == null) && (parameter.Value == null)) ||
                (result.Value.Equals(parameter.Value))))
            {
                return _parameters.Remove(parameter.Name);
            }

            return false;
        }

        /// <summary>Clears the parameters collection.</summary>
        [ExcludeFromCodeCoverage]
        public void Clear()
        {
            _parameters.Clear();
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        IEnumerator<HeaderParameter> IEnumerable<HeaderParameter>.GetEnumerator()
        {
            return _parameters.Values.GetEnumerator();
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        bool ICollection<HeaderParameter>.Contains(HeaderParameter item)
        {
            HeaderParameter result;
            return (_parameters.TryGetValue(item.Name, out result) && Equals(result.Value, item.Value));
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        void ICollection<HeaderParameter>.CopyTo(HeaderParameter[] array, int arrayIndex)
        {
            _parameters.Values.ToArray().CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _parameters.Values.GetEnumerator();
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return String.Join(";", _parameters.Values);
        }
    }
}