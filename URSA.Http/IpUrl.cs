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
                throw new InvalidOperationException(String.Format("Url of type '{0}' does not support segment operations.", GetType()));
            }

            return (String.IsNullOrEmpty(segment) ? this : AddSegments(new[] { segment }));
        }

        /// <summary>Adds segments to the URL.</summary>
        /// <param name="segments">The segments.</param>
        /// <returns>New <see cref="IpUrl" /> instance with segments added.</returns>
        public virtual IpUrl AddSegments(IEnumerable<string> segments)
        {
            return InsertSegments(Int32.MaxValue, segments);
        }

        /// <summary>Inserts segment to the URL.</summary>
        /// <param name="index">Index into which new segments should be inserted.</param>
        /// <param name="segment">The segment.</param>
        /// <returns>New <see cref="IpUrl" /> instance with segment inserted.</returns>
        public virtual IpUrl InsertSegment(int index, string segment)
        {
            if (!SupportsSegmentOperations)
            {
                throw new InvalidOperationException(String.Format("Url of type '{0}' does not support segment operations.", GetType()));
            }

            return (String.IsNullOrEmpty(segment) ? this : InsertSegments(index, new[] { segment }));
        }

        /// <summary>Inserts segments to the URL.</summary>
        /// <param name="index">Index into which new segments should be inserted.</param>
        /// <param name="segments">The segments.</param>
        /// <returns>New <see cref="IpUrl" /> instance with segments inserted.</returns>
        public virtual IpUrl InsertSegments(int index, IEnumerable<string> segments)
        {
            if (!SupportsSegmentOperations)
            {
                throw new InvalidOperationException(String.Format("Url of type '{0}' does not support segment operations.", GetType()));
            }

            segments = segments.Where(segment => !String.IsNullOrEmpty(segment));
            if (!segments.Any())
            {
                return this;
            }

            var newSegments = new List<string>();
            int currentIndex = 0;
            index = Math.Max(index, 0);
            bool segmentsInserted = false;
            foreach (string segment in Segments)
            {
                if (currentIndex != index)
                {
                    newSegments.Add(segment);
                }
                else
                {
                    newSegments.AddRange(segments);
                    newSegments.Add(segment);
                    segmentsInserted = true;
                }

                currentIndex++;
            }

            if (!segmentsInserted)
            {
                newSegments.AddRange(segments);
            }

            return CreateInstance(newSegments);
        }

        /// <summary>Removes last segment.</summary>
        /// <remarks>If there is no <paramref name="predicate" /> provided, last segment is removed by default.</remarks>
        /// <param name="predicate">Optional predicate used determine which segments to remove.</param>
        /// <returns>New <see cref="IpUrl" /> instance with segment removed.</returns>
        public virtual IpUrl RemoveSegment(Func<string, int, bool> predicate = null)
        {
            if (!SupportsSegmentOperations)
            {
                throw new InvalidOperationException(String.Format("Url of type '{0}' does not support segment operations.", GetType()));
            }

            int count = Segments.Count();
            return (count == 0 ? this : CreateInstance(predicate == null ? 
                Segments.TakeWhile((segment, index) => index < count - 1) : 
                Segments.Where((segment, index) => !predicate(segment, index))));
        }

        /// <summary>Adds a key-value parameter pair.</summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>URL with parameter added.</returns>
        public virtual IpUrl WithParameter(string key, string value)
        {
            var result = CreateInstance(Segments, true);
            result.Parameters.AddValue(key, value);
            return result;
        }

        /// <summary>Creates a copy of this URL without parameters.</summary>
        /// <returns>Copy of this URL without parameters.</returns>
        public virtual IpUrl WithoutParameters()
        {
            return CreateInstance(Segments, null);
        }

        /// <summary>Creates a new instance with new collection of segments.</summary>
        /// <param name="segments">The segments that should be used to build a new path.</param>
        /// <param name="requiresParameters">Flag indicating whether the resulting instance will require the parameters collection to be present if <b>true</b>, doesn't care if <b>false</b> or must not be present if <b>null</b>.</param>
        /// <returns>New instance of the URL.</returns>
        protected abstract IpUrl CreateInstance(IEnumerable<string> segments, bool? requiresParameters = false);
    }
}