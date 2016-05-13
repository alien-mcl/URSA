using System;
using System.Collections.Generic;

namespace URSA.Web.Http.Tests.Testing
{
    /// <summary>Describes an <see cref="Url" /> test scenario.</summary>
    public class UrlScenario
    {
        /// <summary>Initializes a new instance of the <see cref="UrlScenario"/> class.</summary>
        public UrlScenario()
        {
            Scheme = UserName = Password = Host = Path = String.Empty;
            Parameters = null;
            Fragment = null;
            Port = 0;
        }

        /// <summary>Gets or sets the scheme.</summary>
        public string Scheme { get; set; }

        /// <summary>Gets or sets the name of the user.</summary>
        public string UserName { get; set; }

        /// <summary>Gets or sets the password.</summary>
        public string Password { get; set; }

        /// <summary>Gets or sets the host.</summary>
        public string Host { get; set; }

        /// <summary>Gets or sets the port.</summary>
        public ushort Port { get; set; }

        /// <summary>Gets or sets the path.</summary>
        public string Path { get; set; }

        /// <summary>Gets or sets the query.</summary>
        public IEnumerable<KeyValuePair<string, string>> Parameters { get; set; }

        /// <summary>Gets or sets the fragment.</summary>
        public string Fragment { get; set; }

        /// <summary>Gets or sets a string representation.</summary>
        public string AsString { get; set; }
    }
}