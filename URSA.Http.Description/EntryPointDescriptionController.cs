using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.NamedGraphs;

namespace URSA.Web.Http.Description
{
    /// <summary>Generates an API entry point description.</summary>
    public class EntryPointDescriptionController : DescriptionController
    {
        private readonly IApiEntryPointDescriptionBuilder _apiDescriptionBuilder;

        /// <summary>Initializes a new instance of the <see cref="EntryPointDescriptionController"/> class.</summary>
        /// <param name="entryPoint">Entry point URL.</param>
        /// <param name="entityContextProvider">Entity context provider.</param>
        /// <param name="apiDescriptionBuilder">API description builder.</param>
        /// <param name="namedGraphSelectorFactory">Named graph selector factory.</param>
        [ExcludeFromCodeCoverage]
        public EntryPointDescriptionController(
            Url entryPoint,
            IEntityContextProvider entityContextProvider,
            IApiEntryPointDescriptionBuilder apiDescriptionBuilder,
            INamedGraphSelectorFactory namedGraphSelectorFactory) :
            base(entityContextProvider, apiDescriptionBuilder, namedGraphSelectorFactory)
        {
            if (entryPoint == null)
            {
                throw new ArgumentNullException("entryPoint");
            }

            (_apiDescriptionBuilder = apiDescriptionBuilder).EntryPoint = entryPoint;
        }

        /// <summary>Gets the entry point.</summary>
        [ExcludeFromCodeCoverage]
        public Url EntryPoint { get { return _apiDescriptionBuilder.EntryPoint; } }

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