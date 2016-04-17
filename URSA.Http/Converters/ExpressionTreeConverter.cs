using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using RomanticWeb;
using RomanticWeb.Entities;
using URSA.Web.Converters;
using URSA.Web.Http.Description.CodeGen;

namespace URSA.Web.Http.Converters
{
    /// <summary>Provides a conversion facility for OData filter.</summary>
    public class ExpressionTreeConverter : IConverter
    {
        private static readonly string[] MediaTypes = { BinaryConverter.AnyAny };

        private readonly IEnumerable<IUriParser> _uriParsers;
        private readonly IEntityContextFactory _entityContextFactory;

        /// <summary>Initializes a new instance of the <see cref="ExpressionTreeConverter"/> class.</summary>
        /// <param name="uriParsers">The URI parsers.</param>
        /// <param name="entityContextFactory">Entity context factory.</param>
        public ExpressionTreeConverter(IEnumerable<IUriParser> uriParsers, IEntityContextFactory entityContextFactory)
        {
            _uriParsers = uriParsers;
            _entityContextFactory = entityContextFactory;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public IEnumerable<string> SupportedMediaTypes { get { return MediaTypes; } }

        /// <inheritdoc />
        public CompatibilityLevel CanConvertTo<T>(IRequestInfo request)
        {
            return CanConvertTo(typeof(T), request);
        }

        /// <inheritdoc />
        public CompatibilityLevel CanConvertTo(Type expectedType, IRequestInfo request)
        {
            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if ((!typeof(Expression).IsAssignableFrom(expectedType)) || (!expectedType.IsGenericType) || (!expectedType.GetGenericArguments()[0].IsGenericType) || 
                (expectedType.GetGenericArguments()[0].GetGenericArguments()[0].GetImplementationOfAny(typeof(IControlledEntity<>)) == null))
            {
                return CompatibilityLevel.None;
            }

            return CompatibilityLevel.ExactTypeMatch | CompatibilityLevel.ExactProtocolMatch;
        }

        /// <inheritdoc />
        public T ConvertTo<T>(IRequestInfo request)
        {
            return (T)ConvertTo(typeof(T), request);
        }

        /// <inheritdoc />
        public object ConvertTo(Type expectedType, IRequestInfo request)
        {
            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            string content;
            using (var reader = new StreamReader(request.Body))
            {
                content = reader.ReadToEnd();
            }

            return ConvertTo(expectedType, content);
        }

        /// <inheritdoc />
        public T ConvertTo<T>(string body)
        {
            return (T)ConvertTo(typeof(T), body);
        }

        /// <inheritdoc />
        public object ConvertTo(Type expectedType, string body)
        {
            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            if (body == null)
            {
                return (expectedType.IsValueType ? Activator.CreateInstance(expectedType) : null);
            }

            var entityType = expectedType.GetGenericArguments()[0].GetGenericArguments()[0];
            return Sprint.Filter.OData.Filter.Deserialize(entityType, ParsePropertyUris(entityType, body));
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public CompatibilityLevel CanConvertFrom<T>(IResponseInfo response)
        {
            return CanConvertFrom(typeof(T), response);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public CompatibilityLevel CanConvertFrom(Type givenType, IResponseInfo response)
        {
            return CompatibilityLevel.None;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public void ConvertFrom<T>(T instance, IResponseInfo response)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public void ConvertFrom(Type givenType, object instance, IResponseInfo response)
        {
            throw new NotImplementedException();
        }

        private string ParsePropertyUris(Type entityType, string body)
        {
            return Regex.Replace(body, "(?<Uri>\\<[^>]+\\>)", match => GetProperty(entityType, match));
        }

        private string GetProperty(Type entityType, Match match)
        {
            var uri = new Uri(match.Groups["Uri"].Value.Trim('<', '>'), UriKind.Absolute);
            return (typeof(IEntity).IsAssignableFrom(entityType) ? GetPropertyMapping(uri) : GetPropertyName(uri));
        }

        private string GetPropertyMapping(Uri uri)
        {
            var mapping = _entityContextFactory.Mappings.MappingForProperty(uri);
            if (mapping == null)
            {
                throw new ArgumentException(String.Format("Unknown property mapped with URI '{0}'.", uri));
            }

            return mapping.Name;            
        }

        private string GetPropertyName(Uri uri)
        {
            var parser = GetBestUriParser(uri);
            if (parser == null)
            {
                throw new InvalidOperationException(String.Format("Cannot parse property Uri '{0}'.", uri));
            }

            string @namespace;
            return parser.Parse(uri, out @namespace);
        }

        private IUriParser GetBestUriParser(Uri uri)
        {
            return (from uriParser in _uriParsers
                    let compatibility = uriParser.IsApplicable(uri)
                    where compatibility != UriParserCompatibility.None
                    orderby (int)compatibility descending
                    select uriParser).FirstOrDefault();
        }
    }
}
