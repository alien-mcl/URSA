using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RomanticWeb;

namespace URSA.Web.Http.Description
{
    /// <summary>Generates an API entry point description.</summary>
    public class EntryPointDescriptionController : DescriptionController
    {
        private readonly IApiEntryPointDescriptionBuilder _apiDescriptionBuilder;

        /// <summary>Initializes a new instance of the <see cref="EntryPointDescriptionController"/> class.</summary>
        /// <param name="entryPoint">Entry point Uri.</param>
        /// <param name="entityContext">Entity context.</param>
        /// <param name="apiDescriptionBuilder">API description builder.</param>
        [ExcludeFromCodeCoverage]
        public EntryPointDescriptionController(Uri entryPoint, IEntityContext entityContext, IApiEntryPointDescriptionBuilder apiDescriptionBuilder) : base(entityContext, apiDescriptionBuilder)
        {
            if (entryPoint == null)
            {
                throw new ArgumentNullException("entryPoint");
            }

            (_apiDescriptionBuilder = apiDescriptionBuilder).EntryPoint = entryPoint;
        }

        /// <summary>Gets the entry point.</summary>
        [ExcludeFromCodeCoverage]
        public Uri EntryPoint { get { return _apiDescriptionBuilder.EntryPoint; } }

        /// <inheritdoc />
        protected internal override string FileName
        {
            get
            {
                var entryPoint = EntryPoint.ToString();
                int position = entryPoint.IndexOf('#');
                return (position != -1 ? entryPoint.Substring(position + 1) : entryPoint.Split('/').Last());
            }
        }
    }
}