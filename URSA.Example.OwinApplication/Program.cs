using System;
#if CORE
using Microsoft.AspNetCore.Hosting;
#endif

#if CORE
#else
using Microsoft.Owin.Hosting;
#endif

namespace URSA.Example.OwinApplication
{
    /// <summary>Provides a basic HTTP hosting.</summary>
    public static class Program
    {
        /// <summary>Main entry point of the hosting application.</summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            Uri baseUri = null;
            if ((args.Length > 0) && (args[0].StartsWith("/uri=")) && (args[0].Length > 6))
            {
                Uri.TryCreate(args[0].Substring(5), UriKind.Absolute, out baseUri);
            }

            if (baseUri == null)
            {
                Console.WriteLine("Missing or invalid base URI. Usage:{0}\tURSA.Example.OwinApplication.exe /uri=http://HOST[:PORT]{0}where{0}\tHOST - a host name{0}\tPORT - optional port number");
                Environment.Exit(0);
            }

            Console.WriteLine("Starting HTTP server at {0}.", baseUri);
#if CORE
            var server = new WebHostBuilder().UseKestrel().UseUrls(baseUri.ToString()).UseStartup<Startup>().Build();
            server.Run();
#else
            var server = WebApp.Start<Startup>(baseUri.ToString());
#endif
            Console.WriteLine("Started HTTP server at {0}.", baseUri);
            Console.ReadLine();
            Console.WriteLine("Stopping HTTP server at {0}.", baseUri);
            server.Dispose();
            Console.WriteLine("Stopped HTTP server at {0}.", baseUri);
        }
    }
}