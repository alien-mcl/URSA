using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace URSA.Web.Http
{
    /// <summary>Describes an HTTP header parameter.</summary>
    public class HeaderParameter
    {
        /// <summary>Defines a 'boundary' parameter name.</summary>
        public const string Boundary = "boundary";

        private const NumberStyles AllowedNumberStyles = NumberStyles.Number | NumberStyles.Integer | NumberStyles.Float;

        /// <summary>Initializes a new instance of the <see cref="HeaderParameter" /> class.</summary>
        /// <param name="name">Name of the parameter.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public HeaderParameter(string name) : this(name, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="HeaderParameter" /> class.</summary>
        /// <param name="name">Name of the parameter.</param>
        /// <param name="value">Value of the parameter.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public HeaderParameter(string name, object value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (name.Length == 0)
            {
                throw new ArgumentOutOfRangeException("name");
            }

            Name = name;
            Value = value;
        }

        /// <summary>Gets the name of the parameter.</summary>
        public string Name { get; private set; }

        /// <summary>Gets or sets the value of the parameter.</summary>
        public object Value { get; set; }

        /// <summary>Tries to parse the HTTP header parameter.</summary>
        /// <param name="parameter">Parameter string to be parsed.</param>
        /// <param name="headerParameter">Resulting parameter if successfully parsed; otherwise <b>null</b>.</param>
        /// <returns><b>true</b> if the parsing was successful; otherwise <b>false</b>.</returns>
        public static bool TryParse(string parameter, out HeaderParameter headerParameter)
        {
            bool result = false;
            headerParameter = null;
            try
            {
                headerParameter = Parse(parameter);
                result = true;
            }
            catch
            {
            }

            return result;
        }

        /// <summary>Parses given string as the parameter.</summary>
        /// <param name="parameter">Parameter string to be parsed.</param>
        /// <returns>Instance of the <see cref="HeaderParameter" />.</returns>
        public static HeaderParameter Parse(string parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            if (parameter.Length == 0)
            {
                throw new ArgumentOutOfRangeException("parameter");
            }

            string[] parts = parameter.Split('=');
            string key = parts[0].Trim();
            if (key.Length == 0)
            {
                throw new ArgumentOutOfRangeException("parameter");
            }

            string rest = (parts.Length > 1 ? parameter.Substring(parts[0].Length + 1).Trim() : null);
            if ((rest != null) && (rest.Length == 0))
            {
                throw new InvalidOperationException("Missing value of the parameter");
            }

            return ParseInternal(key, rest);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string value = String.Empty;
            if (Value != null)
            {
                value = String.Format(CultureInfo.InvariantCulture, "={1}{0}{1}", Value, Value is String ? "\"" : String.Empty);
            }

            return String.Format(CultureInfo.InvariantCulture, "{0}{1}", Name, value);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if ((Object.Equals(obj, null)) || (!(obj is HeaderParameter)))
            {
                return false;
            }

            HeaderParameter parameter = (HeaderParameter)obj;
            return Name.Equals(parameter.Name) && (Value != null ? Value.Equals(parameter.Value) : parameter.Value == null);
        }

        private static HeaderParameter ParseInternal(string key, string rest)
        {
            HeaderParameter result = new HeaderParameter(key);
            if (rest == null)
            {
                return result;
            }

            if ((rest.Length > 2) && (rest[0] == '"') && (rest[rest.Length - 1] == '"'))
            {
                result.Value = rest.Trim('"').Unescape();
            }
            else if ((rest.Length > 2) && (rest[0] == '<') && (rest[rest.Length - 1] == '>'))
            {
                result.Value = new Uri(rest.Trim('<', '>').Unescape());
            }
            else
            {
                double numericValue;
                result.Value = (Double.TryParse(rest, AllowedNumberStyles, CultureInfo.InvariantCulture, out numericValue) ? 
                    numericValue :
                    AppDomain.CurrentDomain.GetAssemblies().FindEnumValue(rest)) ?? rest;
            }

            return result;
        }
    }
}