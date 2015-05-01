using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace URSA.Web.Http
{
    /// <summary>Represents an HTTP header value.</summary>
    public class HeaderValue
    {
        /// <summary>Defines a 'form-data' header value.</summary>
        public const string FormData = "form-data";

        /// <summary>Initializes a new instance of the <see cref="HeaderValue" /> class.</summary>
        /// <param name="value">Actual value.</param>
        /// <param name="parameters">Optional parameters.</param>
        [ExcludeFromCodeCoverage]
        public HeaderValue(string value, params HeaderParameter[] parameters)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Value = value;
            Parameters = new HeaderParameterCollection();
            parameters.ForEach(parameter => Parameters.Add(parameter));
        }

        /// <summary>Initializes a new instance of the <see cref="HeaderValue" /> class.</summary>
        /// <param name="value">Actual value.</param>
        /// <param name="parameters">Optional parameters.</param>
        [ExcludeFromCodeCoverage]
        public HeaderValue(string value, HeaderParameterCollection parameters)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Value = value;
            Parameters = parameters ?? new HeaderParameterCollection();
        }

        /// <summary>Gets the actual value.</summary>
        public string Value { get; protected set; }

        /// <summary>Gets the parameters.</summary>
        public HeaderParameterCollection Parameters { get; private set; }

        /// <summary>Gets the parameters string.</summary>
        public string Parameter { get { return Parameters.ToString(); } }

        /// <summary>Tries to parse a given string as an <see cref="HeaderValue" />.</summary>
        /// <param name="value">String to be parsed.</param>
        /// <param name="headerValue">Resulting header value if parsing was successful; otherwise <b>null</b>.</param>
        /// <returns><b>true</b> if parsing was successful; otherwise <b>false</b>.</returns>
        public static bool TryParse(string value, out HeaderValue headerValue)
        {
            bool result = false;
            headerValue = null;
            try
            {
                headerValue = Parse(value);
                result = true;
            }
            catch
            {
            }

            return result;
        }

        /// <summary>Parses a given string as an <see cref="HeaderValue" />.</summary>
        /// <param name="value">String to be parsed.</param>
        /// <returns>Instance of the <see cref="HeaderValue" />.</returns>
        public static HeaderValue Parse(string value)
        {
            return ParseInternal(null, value);
        }

        /// <summary>Checks if the given header value equals a literal.</summary>
        /// <param name="value">Header value.</param>
        /// <param name="someValue">Literal to be checked against.</param>
        /// <returns><b>true</b> if both arguments are <b>null</b> or <see cref="HeaderValue.Value" /> property of the <paramref name="value" /> equals <paramref name="someValue" />; otherwise <b>false</b>.</returns>
        public static bool operator ==(HeaderValue value, string someValue)
        {
            return ((Object.Equals(value, null)) && (Object.Equals(someValue, null))) ||
                ((!Object.Equals(value, null)) && (!Object.Equals(someValue, null)) && (value.Equals(Parse(someValue))));
        }

        /// <summary>Checks if the given header value is not equal a literal.</summary>
        /// <param name="value">Header value.</param>
        /// <param name="someValue">Literal to be checked against.</param>
        /// <returns><b>false</b> if both arguments are <b>null</b> or <see cref="HeaderValue.Value" /> property of the <paramref name="value" /> equals <paramref name="someValue" />; otherwise <b>true</b>.</returns>
        public static bool operator !=(HeaderValue value, string someValue)
        {
            return !(value == someValue);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override int GetHashCode()
        {
            return Value.GetHashCode() ^ Parameters.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is HeaderValue))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            HeaderValue value = (HeaderValue)obj;
            return Value.Equals(value.Value) && (Parameters.SequenceEqual(value.Parameters));
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            StringBuilder result = new StringBuilder(256);
            result.Append(Value);
            if (Parameters.Count <= 0)
            {
                return result.ToString();
            }

            result.Append(";");
            result.Append(Parameter);
            return result.ToString();
        }

        internal static HeaderValue ParseInternal(string header, string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            HeaderParameterCollection parameters = new HeaderParameterCollection();
            StringBuilder currentValue = new StringBuilder(64);
            StringBuilder currentParameter = new StringBuilder(128);
            StringBuilder currentTarget = currentValue;
            bool isInString = false;
            bool isEscape = false;
            foreach (char letter in value)
            {
                switch (letter)
                {
                    default:
                        ParseOtherChars(letter, currentTarget, ref isInString, ref isEscape);
                        break;
                    case '\\':
                        ParseEscapeChar(letter, currentTarget, ref isInString, ref isEscape);
                        break;
                    case '"':
                        ParseStringChar(letter, currentTarget, ref isInString, ref isEscape);
                        break;
                    case ';':
                        ParseSeparatorChar(letter, ref currentTarget, ref isInString, ref isEscape, currentValue, currentParameter, parameters);
                        break;
                }
            }

            if (currentParameter.Length > 0)
            {
                parameters.Add(HeaderParameter.Parse(currentParameter.ToString().Trim()));
            }

            return CreateInstance(header, currentValue.ToString(), parameters);
        }

        private static HeaderValue CreateInstance(string header, string value, HeaderParameterCollection parameters)
        {
            switch (header)
            {
                case Header.ContentLength:
                    return new HeaderValue<int>((value.Length > 0 ? Int32.Parse(value) : 0), parameters);
                default:
                    return new HeaderValue(value, parameters);
            }
        }

        private static void ParseOtherChars(char chr, StringBuilder currentTarget, ref bool isInString, ref bool isEscape)
        {
            if (isEscape)
            {
                isEscape = false;
                currentTarget.Append("\\" + chr);
            }
            else
            {
                currentTarget.Append(chr);
            }
        }

        private static void ParseEscapeChar(char chr, StringBuilder currentTarget, ref bool isInString, ref bool isEscape)
        {
            if (isEscape)
            {
                isEscape = false;
                currentTarget.Append("\\" + chr);
            }
            else
            {
                isEscape = true;
            }
        }

        private static void ParseStringChar(char chr, StringBuilder currentTarget, ref bool isInString, ref bool isEscape)
        {
            if (isEscape)
            {
                isEscape = false;
                currentTarget.Append("\\" + chr);
            }
            else
            {
                isInString = !isInString;
                currentTarget.Append(chr);
            }
        }

        private static void ParseSeparatorChar(char chr, ref StringBuilder currentTarget, ref bool isInString, ref bool isEscape, StringBuilder currentValue, StringBuilder currentParameter, HeaderParameterCollection parameters)
        {
            if (isEscape)
            {
                isEscape = false;
                currentTarget.Append("\\" + chr);
            }
            else if (isInString)
            {
                currentTarget.Append(chr);
            }
            else
            {
                if (currentTarget == currentValue)
                {
                    currentTarget = currentParameter;
                }
                else
                {
                    parameters.Add(HeaderParameter.Parse(currentParameter.ToString().Trim()));
                    currentParameter.Clear();
                }
            }
        }
    }

    /// <summary>Represents a header value that is provided in a native type.</summary>
    /// <typeparam name="T">Type of the value</typeparam>
    public class HeaderValue<T> : HeaderValue
    {
        private readonly TypeConverter _typeConverter;

        /// <summary>Initializes a new instance of the <see cref="HeaderValue" /> class.</summary>
        /// <param name="value">Actual value.</param>
        /// <param name="parameters">Optional parameters.</param>
        [ExcludeFromCodeCoverage]
        public HeaderValue(T value, params HeaderParameter[] parameters) : base((value != null ? value.ToString() : null), parameters)
        {
            _typeConverter = TypeDescriptor.GetConverter(typeof(T));
            if ((_typeConverter == null) || (!_typeConverter.CanConvertFrom(typeof(string))))
            {
                throw new ArgumentOutOfRangeException("T");
            }
        }

        /// <summary>Initializes a new instance of the <see cref="HeaderValue" /> class.</summary>
        /// <param name="value">Actual value.</param>
        /// <param name="parameters">Optional parameters.</param>
        [ExcludeFromCodeCoverage]
        public HeaderValue(T value, HeaderParameterCollection parameters) : base((value != null ? value.ToString() : null), parameters)
        {
            _typeConverter = TypeDescriptor.GetConverter(typeof(T));
            if ((_typeConverter == null) || (!_typeConverter.CanConvertFrom(typeof(string))))
            {
                throw new ArgumentOutOfRangeException("T");
            }
        }

        /// <summary>Gets or sets the native value of type <typeparam name="T" />.</summary>
        public new T Value
        {
            get { return (T)(base.Value.Length > 0 ? _typeConverter.ConvertFromInvariantString(base.Value) : default(T)); }
            set { base.Value = (value != null ? value.ToString() : null); }
        }
    }
}