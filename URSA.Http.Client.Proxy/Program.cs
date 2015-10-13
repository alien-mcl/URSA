using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RomanticWeb;
using URSA.CodeGen;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web.Converters;
using URSA.Web.Http;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Client.Proxy
{
    public class Program
    {
        private const string Method = "OPTIONS";
        private static IComponentProvider _container;

        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Write("Usage{0}\tURSAHttpClientProxyGenerator http://my.api/rest/service target-directory", Environment.NewLine);
                Environment.Exit(0);
            }

            Process(args[0], args[1]);
        }

        private static void Process(string uri, string targetDirectory)
        {
            Uri url;
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (uri.Length == 0)
            {
                throw new ArgumentOutOfRangeException("uri");
            }

            if (targetDirectory == null)
            {
                throw new ArgumentNullException("targetDirectory");
            }

            if (targetDirectory.Length == 0)
            {
                throw new ArgumentOutOfRangeException("targetDirectory");
            }

            url = new Uri(uri);
            if (!url.Scheme.StartsWith("http"))
            {
                throw new ArgumentOutOfRangeException("uri");
            }

            if (!Directory.Exists(targetDirectory))
            {
                throw new ArgumentOutOfRangeException("targetDirectory");
            }

            _container = UrsaConfigurationSection.InitializeComponentProvider();
            CreateProxy(targetDirectory, url);
        }

        private static IApiDocumentation GetApiDocumentation(Uri url)
        {
            string contentType;
            var responseStream = new UnclosableStream(GetResponse(Method, url, out contentType));
            var container = UrsaConfigurationSection.InitializeComponentProvider();
            var headers = new HeaderCollection();
            if (!String.IsNullOrEmpty(contentType))
            {
                ((IDictionary<string, string>)headers)[Header.Accept] = contentType;
                ((IDictionary<string, string>)headers)[Header.ContentType] = contentType;
            }

            var httpRequest = new RequestInfo(Verb.Parse(Method), new Uri(url + "/#"), responseStream, headers);
            var converterProvider = container.Resolve<IConverterProvider>();
            var converter = converterProvider.FindBestInputConverter<IApiDocumentation>(httpRequest);
            if (converter == null)
            {
                throw new NotSupportedException(String.Format("Content type of '{0}' is not supported.", contentType));
            }

            return converter.ConvertTo<IApiDocumentation>(httpRequest);
        }

        private static Stream GetResponse(string method, Uri url, out string contentType)
        {
            var request = WebRequest.CreateHttp(url);
            request.Method = method;
            request.UserAgent = "URSA Proxy Generator";
            request.Accept = String.Join(", ", EntityConverter.MediaTypes);
            var response = (HttpWebResponse)request.GetResponse();
            contentType = response.Headers[HttpResponseHeader.ContentType];
            return response.GetResponseStream();
        }

        private static void CreateProxy(string targetDirectory, Uri url)
        {
            var apiDocumentation = GetApiDocumentation(url);
            if (apiDocumentation == null)
            {
                throw new InvalidOperationException(String.Format("No API documentation found at '{0}'.", url));
            }

            if (apiDocumentation.SupportedClasses.Count == 0)
            {
                throw new InvalidOperationException(String.Format("No supported classes found at '{0}'.", url));
            }

            var generator = _container.Resolve<IClassGenerator>();
            foreach (var @class in apiDocumentation.SupportedClasses.Select(supportedClass => generator.CreateCode(supportedClass)).SelectMany(classes => classes))
            {
                File.WriteAllText(Path.Combine(targetDirectory, @class.Key), @class.Value);
            }
        }
    }
}