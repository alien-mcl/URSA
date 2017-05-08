using System.Collections.Generic;
using RDeF.Mapping.Attributes;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Represents a hyperlink.</summary>
    [Class("hydra", "Link")]
    public interface ILink : Rdfs.IProperty, ISupportedOperationsOwner
    {
    }
}