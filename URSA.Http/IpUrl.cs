using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace URSA.Web.Http
{
    /// <summary>Represents a common internet resource locator.</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
    public abstract class IpUrl : Url
    {
        private readonly string _url;
        private readonly string _scheme;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _host;
        private readonly ushort _port;

        internal IpUrl(
            string url,
            string scheme,
            string userName,
            string password,
            string host,
            ushort port)
        {
            _url = url;
            _scheme = scheme;
            _userName = userName ?? String.Empty;
            _password = password ?? String.Empty;
            _host = host;
            _port = port;
        }

        /// <inheritdoc />
        public virtual bool IsAbsolute { get { return true; } }

        /// <inheritdoc />
        public override string Scheme { get { return _scheme; } }

        /// <inheritdoc />
        public string UserName { get { return _userName; } }

        /// <inheritdoc />
        public string Password { get { return _password; } }

        /// <inheritdoc />
        public override string Host { get { return _host; } }

        /// <inheritdoc />
        public override ushort Port { get { return _port; } }

        /// <inheritdoc />
        public virtual string Path { get { return "/"; } }

        /// <inheritdoc />
        public abstract ParametersCollection Parameters { get; }

        /// <summary>Gets a value indicating whether this instance has a query.</summary>
        public bool HasParameters { get { return Parameters != null; } }

        /// <inheritdoc />
        public abstract IEnumerable<string> Segments { get; }

        /// <inheritdoc />
        public override string OriginalUrl { get { return _url; } }

        /// <summary>Gets a value indicating whether the URL supports segment operations, i.e. adding or removing them.</summary>
        public virtual bool SupportsSegmentOperations { get { return true; } }

        /// <summary>Adds the segment to the URL.</summary>
        /// <param name="segment">The segment.</param>
        /// <returns>New <see cref="IpUrl" /> instance with segment added.</returns>
        public virtual IpUrl AddSegment(string segment)
        {
            if (!SupportsSegmentOperations)
            {
                throw new InvalidOperationException(String.Format("Url of type '{0}' does not support segment operations."));
            }

            return (String.IsNullOrEmpty(segment) ? this : CreateInstance(Segments.Concat(new[] { segment })));
        }

        /// <summary>Removes last segment.</summary>
        /// <returns>New <see cref="IpUrl" /> instance with segment removed.</returns>
        public virtual IpUrl RemoveSegment()
        {
            if (!SupportsSegmentOperations)
            {
                throw new InvalidOperationException(String.Format("Url of type '{0}' does not support segment operations."));
            }

            int count = Segments.Count();
            return (count == 0 ? this : CreateInstance(Segments.TakeWhile((segment, index) => index < count - 1)));
        }

        /// <summary>Creates a new instance with new collection of segments.</summary>
        /// <param name="segments">The segments that should be used to build a new path.</param>
        /// <returns>New instance of the URL.</returns>
        protected abstract IpUrl CreateInstance(IEnumerable<string> segments);
    }
}