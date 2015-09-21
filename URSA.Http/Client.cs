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
using URSA.Web.Http.Mapping;

namespace URSA.Web.Http
{
    /// <summary>Acts as a base class for HTTP client proxies.</summary>
    public class Client
    {
        private static readonly string[] AllowedProtocols = { "http", "https" };

        private readonly IWebRequestProvider _webRequestProvider;
        private readonly IConverterProvider _converterProvider;
        private readonly IResultBinder<RequestInfo> _resultBinder;

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
            _resultBinder = container.Resolve<IResultBinder<RequestInfo>>();
        }

        private Uri BaseUri { get; set; }

        /// <summary>Calls the ReST service using specified HTTP verb.</summary>
        /// <param name="verb">The HTTP verb.</param>
        /// <param name="url">Relative URL of the service to call.</param>
        /// <param name="accept">Enumeration of accepted media types.</param>
        /// <param name="contentType">Enumeration of possible content type media types.</param>
        /// <param name="uriArguments">The URI template arguments.</param>
        /// <param name="bodyArguments">The body arguments.</param>
        protected void Call(Verb verb, string url, IEnumerable<string> accept, IEnumerable<string> contentType, dynamic uriArguments, params object[] bodyArguments)
        {
            Call(verb, url, accept, contentType, uriArguments, bodyArguments);
        }

        /// <summary>Calls the ReST service using specified HTTP verb.</summary>
        /// <param name="verb">The HTTP verb.</param>
        /// <param name="url">Relative URL of the service to call.</param>
        /// <param name="accept">Enumeration of accepted media types.</param>
        /// <param name="contentType">Enumeration of possible content type media types.</param>
        /// <param name="responseType">Type of the response.</param>
        /// <param name="uriArguments">The URI template arguments.</param>
        /// <param name="bodyArguments">The body arguments.</param>
        /// <returns>Result of the call.</returns>
        protected object Call(Verb verb, string url, IEnumerable<string> accept, IEnumerable<string> contentType, Type responseType, dynamic uriArguments, params object[] bodyArguments)
        {
            RequestInfo fakeRequest;
            var uri = BuildUri(url, uriArguments);
            var validAccept = (!accept.Any() ? _converterProvider.SupportedMediaTypes :
                _converterProvider.SupportedMediaTypes.Join(accept, outer => outer, inner => inner, (outer, inner) => inner));
            var accepted = (validAccept.Any() ? validAccept : new[] { "*/*" });
            WebRequest request = _webRequestProvider.CreateRequest(uri, new Dictionary<string, string>() { { Header.Accept, String.Join(", ", accepted) } });
            request.Method = verb.ToString();
            if ((bodyArguments != null) && (bodyArguments.Length > 0))
            {
                fakeRequest = new RequestInfo(verb, uri, new MemoryStream(), new Header(Header.Accept, accepted.ToArray()));
                ResponseInfo fakeResponse = CreateFakeResponseInfo(fakeRequest, contentType, bodyArguments);
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
                        case Header.ContentType:
                            request.ContentType = header.Value;
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

            fakeRequest = new RequestInfo(verb, uri, response.GetResponseStream(), HeaderCollection.Parse(response.Headers.ToString()));
            var result = _resultBinder.BindResults(responseType, fakeRequest);
            return result.FirstOrDefault(responseType.IsInstanceOfType);
        }

        /// <summary>Calls the ReST service using specified HTTP verb.</summary>
        /// <typeparam name="T">Type of the result.</typeparam>
        /// <param name="verb">The HTTP verb.</param>
        /// <param name="url">Relative URL of the service to call.</param>
        /// <param name="mediaTypes">Enumeration of accepted media types.</param>
        /// <param name="contentType">Enumeration of possible content type media types.</param>
        /// <param name="uriArguments">The URI template arguments.</param>
        /// <param name="bodyArguments">The body arguments.</param>
        /// <returns>Result of the call.</returns>
        internal protected T Call<T>(Verb verb, string url, IEnumerable<string> mediaTypes, IEnumerable<string> contentType, dynamic uriArguments, params object[] bodyArguments)
        {
            var result = Call(verb, url, mediaTypes, contentType, typeof(T), uriArguments, bodyArguments);
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

        private ResponseInfo CreateFakeResponseInfo(RequestInfo fakeRequest, IEnumerable<string> contentType, params object[] bodyArguments)
        {
            ResponseInfo result;
            IEnumerable<string> currentContentType;
            if (bodyArguments.Length == 1)
            {
                var singleResponse = ObjectResponseInfo<object>.CreateInstance(Encoding.UTF8, fakeRequest, bodyArguments[0].GetType(), bodyArguments[0], _converterProvider);
                currentContentType = (singleResponse.Headers[Header.ContentType] == null ? new string[0] :
                    new[] { singleResponse.Headers[Header.ContentType].Values.Select(item => item.Value).First() });
                result = singleResponse;
            }
            else
            {
                var multiResponse = new MultiObjectResponseInfo(Encoding.UTF8, fakeRequest, bodyArguments, _converterProvider);
                currentContentType = multiResponse.ObjectResponses.Select(item => item.Headers[Header.ContentType].Values.Select(value => value.Value).First())
                    .Where(item => !String.IsNullOrEmpty(item));
                result = multiResponse;
            }

            if ((contentType.Any()) && (currentContentType.Join(contentType, outer => outer, inner => inner, (outer, inner) => inner).Count() != currentContentType.Count()))
            {
                throw new WebException("Unsupported media type", WebExceptionStatus.ProtocolError);
            }

            return result;
        }
    }
}