using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace URSA.Web.Http
{
    /// <summary>Describes an HTTP header.</summary>
    public class Header
    {
        private static readonly string[] UnparsableHeaders = new string[] { "User-Agent" };

        internal static readonly IEqualityComparer<string> Comparer = StringComparer.OrdinalIgnoreCase;

        /// <summary>Defines the 'Warning' header name.</summary>
        public const string Warning = "Warning";

        /// <summary>Defines the 'Content-Disposition' header name.</summary>
        public const string ContentDisposition = "Content-Disposition";

        /// <summary>Defines the 'Content-Type' header name.</summary>
        public const string ContentType = "Content-Type";

        /// <summary>Defines the 'Content-Length' header name.</summary>
        public const string ContentLength = "Content-Length";

        /// <summary>Defines the 'Content-Encoding' header name.</summary>
        public const string ContentEncoding = "Content-Encoding";

        /// <summary>Defines the 'Accept' header name.</summary>
        public const string Accept = "Accept";

        /// <summary>Defines the 'Accept-Language' header name.</summary>
        public const string AcceptLanguage = "Accept-Language";

        /// <summary>Defines the 'Location' header name.</summary>
        public const string Location = "Location";

        /// <summary>Defines the 'Link' header name.</summary>
        public const string Link = "Link";

        /// <summary>Initializes a new instance of the <see cref="Header" /> class.</summary>
        /// <param name="name">Name of the header.</param>
        /// <param name="values">Value of the header.</param>
        public Header(string name, params string[] values) : this(name, values.Select(value => new HeaderValue(value)).ToArray())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Header" /> class.</summary>
        /// <param name="name">Name of the header.</param>
        /// <param name="values">Value of the header.</param>
        public Header(string name, IEnumerable<HeaderValue> values)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (name.Length == 0)
            {
                throw new ArgumentOutOfRangeException("name");
            }

            if ((Comparer.Equals(name, ContentLength)) && (GetType() != typeof(Header<int>)))
            {
                throw new InvalidOperationException(String.Format("Header 'Content-Length' should be of type '{0}.", typeof(Header<int>)));
            }

            Name = name;
            ValuesCollection = new ObservableCollection<HeaderValue>(values ?? new List<HeaderValue>());
        }

        /// <summary>Gets the header name.</summary>
        public string Name { get; private set; }

        /// <summary>Gets the header values.</summary>
        public ICollection<HeaderValue> Values { get { return ValuesCollection; } }

        /// <summary>Gets the value string.</summary>
        public string Value { get { return String.Join(",", Values); } }

        /// <summary>Gets the values collection as an observable one.</summary>
        protected ObservableCollection<HeaderValue> ValuesCollection { get; private set; }

        /// <summary>Tries to parse a given string as an <see cref="Header" />.</summary>
        /// <param name="header">String to be parsed.</param>
        /// <param name="httpHeader">Resulting HTTP header if parsing was successful; otherwise <b>null</b>.</param>
        /// <returns><b>true</b> if parsing was successful; otherwise <b>false</b>.</returns>
        public static bool TryParse(string header, out Header httpHeader)
        {
            bool result = false;
            httpHeader = null;
            try
            {
                httpHeader = Parse(header);
                result = true;
            }
            catch
            {
            }

            return result;
        }

        /// <summary>Parses a given string as an <see cref="Header" />.</summary>
        /// <param name="header">String to be parsed.</param>
        /// <returns>Instance of the <see cref="Header" />.</returns>
        public static Header Parse(string header)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }

            if (header.Length == 0)
            {
                throw new ArgumentOutOfRangeException("header");
            }

            var match = HeaderCollection.Header.Match(header);
            if ((match.Value.Length == 0) || (match.Groups["Name"] == null) || (match.Groups["Name"].Length == 0))
            {
                throw new ArgumentOutOfRangeException("header");
            }

            IList<HeaderValue> values = new List<HeaderValue>();
            if ((match.Groups["Value"] != null) && (match.Groups["Value"].Value.Length > 0))
            {
                string value = match.Groups["Value"].Value.Trim();
                if (UnparsableHeaders.Contains(match.Groups["Name"].Value))
                {
                    values.Add(new HeaderValue(value));
                }
                else
                {
                    ParseValues(match.Groups["Name"].Value, values, value);
                }
            }

            return CreateInstance(match.Groups["Name"].Value, values);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Values.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is Header))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            Header header = (Header)obj;
            return Name.Equals(header.Name) && (Values.Count == header.Values.Count) && (Values.SequenceEqual(header.Values));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder result = new StringBuilder(256);
            result.Append(Name);
            if (Values.Count > 0)
            {
                result.Append(":");
                result.Append(Value);
            }

            return result.ToString();
        }

        internal static Header CreateInstance(string name, IList<HeaderValue> values)
        {
            if (Comparer.Equals(name, ContentLength))
            {
                return new Header<int>(name, values.Cast<HeaderValue<int>>());
            }
            else
            {
                return new Header(name, values);
            }
        }

        private static void ParseValues(string name, IList<HeaderValue> values, string valuesString)
        {
            StringBuilder value = new StringBuilder(128);
            bool isInString = false;
            bool isEscape = false;
            for (int index = 0; index < valuesString.Length; index++)
            {
                switch (valuesString[index])
                {
                    default:
                        ParseOtherChars(valuesString[index], ref isInString, ref isEscape, value);
                        break;
                    case '\\':
                        ParseEscapeChar(valuesString[index], ref isInString, ref isEscape, value);
                        break;
                    case '"':
                        ParseStringChar(valuesString[index], ref isInString, ref isEscape, value);
                        break;
                    case ',':
                        ParseSeparatorChar(valuesString[index], ref isInString, ref isEscape, value, values);
                        break;
                }
            }

            if (value.Length > 0)
            {
                values.Add(HeaderValue.ParseInternal(name, value.ToString().Trim()));
            }
        }

        private static void ParseOtherChars(char chr, ref bool isInString, ref bool isEscape, StringBuilder value)
        {
            if (isEscape)
            {
                isEscape = false;
                value.Append("\\" + chr);
            }
            else
            {
                value.Append(chr);
            }
        }

        private static void ParseEscapeChar(char chr, ref bool isInString, ref bool isEscape, StringBuilder value)
        {
            if (isEscape)
            {
                isEscape = false;
                value.Append("\\" + chr);
            }
            else
            {
                isEscape = true;
            }
        }

        private static void ParseStringChar(char chr, ref bool isInString, ref bool isEscape, StringBuilder value)
        {
            if (isEscape)
            {
                isEscape = false;
                value.Append("\\" + chr);
            }
            else
            {
                isInString = !isInString;
                value.Append(chr);
            }
        }

        private static void ParseSeparatorChar(char chr, ref bool isInString, ref bool isEscape, StringBuilder value, IList<HeaderValue> values)
        {
            if (isEscape)
            {
                isEscape = false;
                value.Append("\\" + chr);
            }
            else if (isInString)
            {
                value.Append(chr);
            }
            else
            {
                values.Add(HeaderValue.Parse(value.ToString().Trim()));
                value.Clear();
            }
        }
    }

    /// <summary>Represents a header with a value provided in a native type.</summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    public class Header<T> : Header
    {
        private readonly ObservableCollection<HeaderValue<T>> _parsedValues;

        /// <summary>Initializes a new instance of the <see cref="Header" /> class.</summary>
        /// <param name="name">Name of the header.</param>
        /// <param name="values">Value of the header.</param>
        public Header(string name, params T[] values) : this(name, values.Select(value => new HeaderValue<T>(value)).ToArray())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Header" /> class.</summary>
        /// <param name="name">Name of the header.</param>
        /// <param name="values">Value of the header.</param>
        public Header(string name, IEnumerable<HeaderValue<T>> values) : base(
            name, 
            ((typeof(T).IsValueType) && (!values.Any()) ? new HeaderValue<T>[] { new HeaderValue<T>(default(T)) } : values.Cast<HeaderValue>()))
        {
            (_parsedValues = new ObservableCollection<HeaderValue<T>>()).AddRange(values);
            _parsedValues.CollectionChanged += OnParsedValuesCollectionChanged;
            ValuesCollection.CollectionChanged += OnValuesCollectionChanged;
        }

        /// <summary>Gets the header values.</summary>
        public new ICollection<HeaderValue<T>> Values { get { return _parsedValues; } }

        private void OnValuesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems)
            {
                if (!(item is HeaderValue<T>))
                {
                    throw new InvalidOperationException(String.Format("Cannot add item of type '{0}' to values of type '{1}'.", item.GetType(), typeof(T)));
                }

                _parsedValues.Add((HeaderValue<T>)item);
            }

            foreach (var item in e.OldItems)
            {
                _parsedValues.Remove((HeaderValue<T>)item);
            }

            if ((Values.Count == 0) && (typeof(T).IsValueType))
            {
                throw new InvalidOperationException("Cannot remove last value of the value-type based header.");
            }
        }

        private void OnParsedValuesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (HeaderValue<T> item in e.NewItems)
            {
                ValuesCollection.Add(item);
            }

            foreach (HeaderValue<T> item in e.OldItems)
            {
                ValuesCollection.Remove(item);
            }
        }
    }
}