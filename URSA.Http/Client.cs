using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tavis.UriTemplates;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Security;
using URSA.Web.Converters;
using URSA.Web.Http.Collections;
using URSA.Web.Http.Mapping;

namespace URSA.Web.Http
{
    /// <summary>Acts as a base class for HTTP client proxies.</summary>
    public class Client
    {
        private const string DefaultAuthenticationScheme = "Basic";
        private static readonly string[] AllowedProtocols = { "http", "https" };

        private readonly IWebRequestProvider _webRequestProvider;
        private readonly IConverterProvider _converterProvider;
        private readonly IResultBinder<RequestInfo> _resultBinder;
        private readonly IEnumerable<IRequestModelTransformer> _requestModelTransformers;
        private readonly string _authenticationScheme;

        /// <summary>Initializes a new instance of the <see cref="Client"/> class.</summary>
        /// <param name="baseUrl">The base URI.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public Client(HttpUrl baseUrl) : this(baseUrl, DefaultAuthenticationScheme)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Client"/> class.</summary>
        /// <param name="baseUrl">The base URI.</param>
        /// <param name="authenticationScheme">Authentication scheme.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public Client(HttpUrl baseUrl, string authenticationScheme) : this()
        {
            if (baseUrl == null)
            {
                throw new ArgumentNullException("baseUrl");
            }

            if ((authenticationScheme == null) || (authenticationScheme.Length == 0))
            {
                authenticationScheme = DefaultAuthenticationScheme;
            }

            BaseUrl = baseUrl;
            _authenticationScheme = authenticationScheme;
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

            _requestModelTransformers = new DependencyTree<IRequestModelTransformer>(container.ResolveAll<IRequestModelTransformer>(), typeof(IRequestModelTransformer<>));
            _converterProvider = container.Resolve<IConverterProvider>();
            _resultBinder = container.Resolve<IResultBinder<RequestInfo>>();
        }

        /// <summary>Gets the base URL.</summary>
        public HttpUrl BaseUrl { get; private set; }

        internal HttpUrl BuildUrl(string url, IDictionary<string, object> uriArguments)
        {
            var template = new UriTemplate(BaseUrl.ToString() + url);
            foreach (var parameter in uriArguments)
            {
                template.SetParameter(parameter.Key, parameter.Value);
            }

            var result = template.Resolve();
            return (HttpUrl)UrlParser.Parse(Regex.Replace(result.ToString(), "%([0-9]+)", match => Convert.ToChar(UInt32.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString()));
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
        protected internal async Task<T> Call<T>(Verb verb, string url, IEnumerable<string> mediaTypes, IEnumerable<string> contentType, IDictionary<string, object> uriArguments, params object[] bodyArguments)
        {
            var result = await Call(verb, url, mediaTypes, contentType, typeof(T), uriArguments, bodyArguments);
            return (result == null ? default(T) : (result is T ? (T)result : (T)Convert.ChangeType(result, typeof(T))));
        }

        /// <summary>Calls the ReST service using specified HTTP verb.</summary>
        /// <param name="verb">The HTTP verb.</param>
        /// <param name="url">Relative URL of the service to call.</param>
        /// <param name="accept">Enumeration of accepted media types.</param>
        /// <param name="contentType">Enumeration of possible content type media types.</param>
        /// <param name="uriArguments">The URI template arguments.</param>
        /// <param name="bodyArguments">The body arguments.</param>
        /// <returns>Task of this call.</returns>
        protected internal async Task Call(Verb verb, string url, IEnumerable<string> accept, IEnumerable<string> contentType, IDictionary<string, object> uriArguments, params object[] bodyArguments)
        {
            await Call(verb, url, accept, contentType, null, uriArguments, bodyArguments);
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
        protected async Task<object> Call(Verb verb, string url, IEnumerable<string> accept, IEnumerable<string> contentType, Type responseType, IDictionary<string, object> uriArguments, params object[] bodyArguments)
        {
            var callUrl = BuildUrl(url, uriArguments);
            var validAccept = (!accept.Any() ? _converterProvider.SupportedMediaTypes :
                _converterProvider.SupportedMediaTypes.Join(accept, outer => outer, inner => inner, (outer, inner) => inner));
            var accepted = (validAccept.Any() ? validAccept : new[] { "*/*" });
            WebRequest request = _webRequestProvider.CreateRequest((Uri)callUrl, new Dictionary<string, string>() { { Header.Accept, String.Join(", ", accepted) } });
            if ((!String.IsNullOrEmpty(CredentialCache.DefaultNetworkCredentials.UserName)) && (!String.IsNullOrEmpty(CredentialCache.DefaultNetworkCredentials.Password)))
            {
                var credentials = new CredentialCache();
                credentials.Add(new Uri(callUrl.Authority), _authenticationScheme, new NetworkCredential(CredentialCache.DefaultNetworkCredentials.UserName, CredentialCache.DefaultNetworkCredentials.Password));
                request.Credentials = credentials;
#if !CORE
                request.PreAuthenticate = true;
#endif
            }

            request.Method = verb.ToString();
            if ((bodyArguments != null) && (bodyArguments.Length > 0))
            {
                await FillRequestBody(verb, callUrl, request, contentType, accepted, bodyArguments);
            }

            var response = (HttpWebResponse)(await request.GetResponseAsync());
            ParseContentRange(response, uriArguments);
            if (responseType == null)
            {
                return null;
            }

            RequestInfo fakeRequest = new RequestInfo(verb, callUrl, response.GetResponseStream(), new BasicClaimBasedIdentity(), HeaderCollection.Parse(response.Headers.ToString()));
            var result = _resultBinder.BindResults(responseType, fakeRequest);
            return result.FirstOrDefault(responseType.GetTypeInfo().IsInstanceOfType);
        }

        private async Task FillRequestBody(Verb verb, HttpUrl url, WebRequest request, IEnumerable<string> contentType, IEnumerable<string> accepted, params object[] bodyArguments)
        {
            RequestInfo fakeRequest = new RequestInfo(verb, url, new MemoryStream(), new BasicClaimBasedIdentity(), new Header(Header.Accept, accepted.ToArray()));
            foreach (var modelTransformer in _requestModelTransformers)
            {
                bodyArguments = await modelTransformer.Transform(bodyArguments);
            }

            ResponseInfo fakeResponse = CreateFakeResponseInfo(fakeRequest, contentType, bodyArguments);
            using (var target = await request.GetRequestStreamAsync())
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
                        request.Headers[header.Name] = header.Value;
                        break;
                }
            }
        }

        private void ParseContentRange(HttpWebResponse response, IDictionary<string, object> uriArguments)
        {
            try
            {
                if (((!uriArguments.ContainsKey("totalEntities")) || (Equals(uriArguments["totalEntities"], 0))) && (!String.IsNullOrEmpty(response.Headers["Content-Range"])))
                {
                    uriArguments["totalEntities"] = Int32.Parse(Regex.Match(response.Headers["Content-Range"], "[0-9]+\\-[0-9]+/(?<TotalEntities>[0-9]+)").Groups["TotalEntities"].Value);
                }
            }
            catch (FormatException)
            {
                throw;
            }
            catch
            {
                // Ignore any other exceptions.
            }
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