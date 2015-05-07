using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web.Converters;

namespace URSA.Web.Http
{
    /// <summary>Acts as a base class for HTTP client proxies.</summary>
    public class Client
    {
        private static readonly string[] AllowedProtocols = { "http", "https" };

        private readonly IWebRequestProvider _webRequestProvider;
        private readonly IConverterProvider _converterProvider;

        /// <summary>Initializes a new instance of the <see cref="Client"/> class.</summary>
        /// <param name="baseUri">The base URI.</param>
        [ExcludeFromCodeCoverage]
        public Client(Uri baseUri) : this()
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }

            BaseUri = baseUri;
        }

        private Client()
        {
            IComponentProvider container = UrsaConfigurationSection.InitializeComponentProvider();
            _webRequestProvider = (from webRequestProvider in container.ResolveAll<IWebRequestProvider>()
                                   from supportedProtocol in webRequestProvider.SupportedProtocols
                                   join allowedProtocol in AllowedProtocols on supportedProtocol equals allowedProtocol
                                   select webRequestProvider).FirstOrDefault();
            if (_webRequestProvider == null)
            {
                throw new InvalidOperationException("Cannot create an HTTP client without proper web request provider.");
            }

            _converterProvider = container.Resolve<IConverterProvider>();
        }

        private Uri BaseUri { get; set; }

        /// <summary>Calls the ReST service using specified HTTP verb.</summary>
        /// <param name="verb">The HTTP verb.</param>
        /// <param name="url">Relative URL of the service to call.</param>
        /// <param name="mediaTypes">Enumeration of accepted media types.</param>
        /// <param name="uriArguments">The URI template arguments.</param>
        /// <param name="bodyArguments">The body arguments.</param>
        protected void Call(Verb verb, string url, IEnumerable<string> mediaTypes, dynamic uriArguments, params object[] bodyArguments)
        {
            Call(verb, url, mediaTypes, uriArguments, bodyArguments);
        }

        /// <summary>Calls the ReST service using specified HTTP verb.</summary>
        /// <param name="verb">The HTTP verb.</param>
        /// <param name="url">Relative URL of the service to call.</param>
        /// <param name="mediaTypes">Enumeration of accepted media types.</param>
        /// <param name="responseType">Type of the response.</param>
        /// <param name="uriArguments">The URI template arguments.</param>
        /// <param name="bodyArguments">The body arguments.</param>
        /// <returns>Result of the call.</returns>
        protected object Call(Verb verb, string url, IEnumerable<string> mediaTypes, Type responseType, dynamic uriArguments, params object[] bodyArguments)
        {
            RequestInfo fakeRequest;
            var uri = BuildUri(url, uriArguments);
            var accept = String.Join("; ", mediaTypes);
            WebRequest request = _webRequestProvider.CreateRequest(uri, new Dictionary<string, string>() { { Header.Accept, accept } });
            request.Method = verb.ToString();
            if ((bodyArguments != null) && (bodyArguments.Length > 0))
            {
                fakeRequest = new RequestInfo(verb, uri, new MemoryStream(), new Header(Header.Accept, accept));
                var fakeResponse = (bodyArguments.Length == 1 ?
                    ObjectResponseInfo<object>.CreateInstance(Encoding.UTF8, fakeRequest, bodyArguments[0].GetType(), bodyArguments[0], _converterProvider) :
                    new MultiObjectResponseInfo(Encoding.UTF8, fakeRequest, bodyArguments, _converterProvider));
                using (var target = request.GetRequestStream())
                using (var source = fakeResponse.Body)
                {
                    source.CopyTo(target);
                }

                foreach (var header in fakeResponse.Headers)
                {
                    switch (header.Name)
                    {
                        case Header.ContentLength:
                            break;
                        default:
                            request.Headers.Add(header.Name, header.Value);
                            break;
                    }
                }
            }

            var response = (HttpWebResponse)request.GetResponse();
            if (responseType == null)
            {
                return null;
            }

            fakeRequest = new RequestInfo(verb, uri, response.GetResponseStream(), response.Headers.AllKeys.Select(key => new Header(key, response.Headers[key])).ToArray());
            var converter = _converterProvider.FindBestInputConverter(responseType, fakeRequest);
            if (converter == null)
            {
                throw new InvalidOperationException(String.Format("Cannot deserialize response from '{0}'.", uri));
            }

            return converter.ConvertTo(responseType, fakeRequest);
        }

        /// <summary>Calls the ReST service using specified HTTP verb.</summary>
        /// <typeparam name="T">Type of the result.</typeparam>
        /// <param name="verb">The HTTP verb.</param>
        /// <param name="url">Relative URL of the service to call.</param>
        /// <param name="mediaTypes">Enumeration of accepted media types.</param>
        /// <param name="uriArguments">The URI template arguments.</param>
        /// <param name="bodyArguments">The body arguments.</param>
        /// <returns>Result of the call.</returns>
        internal protected T Call<T>(Verb verb, string url, IEnumerable<string> mediaTypes, dynamic uriArguments, params object[] bodyArguments)
        {
            var result = Call(verb, url, mediaTypes, typeof(T), uriArguments, bodyArguments);
            return (result == null ? default(T) : Convert.ChangeType(result, typeof(T)));
        }

        internal Uri BuildUri(string url, dynamic uriArguments)
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