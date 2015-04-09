using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web.Converters;

namespace URSA.Web.Http
{
    /// <summary>Acts as a base class for HTTP client proxies.</summary>
    public class Client
    {
        /// <summary>Initializes a new instance of the <see cref="Client"/> class.</summary>
        /// <param name="baseUri">The base URI.</param>
        [ExcludeFromCodeCoverage]
        public Client(Uri baseUri)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }

            BaseUri = baseUri;
        }

        private Uri BaseUri { get; set; }

        /// <summary>Calls the ReST service using specified HTTP verb.</summary>
        /// <param name="verb">The HTTP verb.</param>
        /// <param name="url">Relative URL of the service to call.</param>
        /// <param name="mediaTypes">Enumeration of accepted media types.</param>
        /// <param name="uriArguments">The URI template arguments.</param>
        /// <param name="bodyArguments">The body arguments.</param>
        /// <returns>Result of the call.</returns>
        protected object Call(Verb verb, string url, IEnumerable<string> mediaTypes, dynamic uriArguments, params object[] bodyArguments)
        {
            var uri = BuildUri(url, uriArguments);
            return null;
        }

        /// <summary>Calls the ReST service using specified HTTP verb.</summary>
        /// <typeparam name="T">Type of the result.</typeparam>
        /// <param name="verb">The HTTP verb.</param>
        /// <param name="url">Relative URL of the service to call.</param>
        /// <param name="mediaTypes">Enumeration of accepted media types.</param>
        /// <param name="uriArguments">The URI template arguments.</param>
        /// <param name="bodyArguments">The body arguments.</param>
        /// <returns>Result of the call.</returns>
        protected T Call<T>(Verb verb, string url, IEnumerable<string> mediaTypes, dynamic uriArguments, params object[] bodyArguments)
        {
            var result = Call(verb, url, uriArguments, bodyArguments);
            return (result == null ? default(T) : Convert.ChangeType(result, typeof(T)));
        }

        private Uri BuildUri(string url, dynamic uriArguments)
        {
            foreach (KeyValuePair<string, object> argument in uriArguments)
            {
                string value = String.Empty;
                if (argument.Value != null)
                {
                    TypeConverter typeConverter = TypeDescriptor.GetConverter(argument.Value.GetType());
                    if ((typeConverter != null) && (typeConverter.CanConvertTo(typeof(string))))
                    {
                        value = typeConverter.ConvertToInvariantString(argument.Value) ?? String.Empty;
                    }
                    else
                    {
                        value = argument.Value.ToString();
                    }
                }

                url = Regex.Replace(url, String.Format("{{\\?{0}}}", argument.Key), value);
            }

            return new Uri(BaseUri, url);
        }
    }
}