using System;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a contract for API entry point description builder facility.</summary>
    public interface IApiEntryPointDescriptionBuilder : IApiDescriptionBuilder
    {
        /// <summary>Gets or sets the entry point.</summary>
        Uri EntryPoint { get; set; }
    }
}