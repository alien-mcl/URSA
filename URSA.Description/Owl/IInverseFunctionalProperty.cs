using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using System.Collections.Generic;

namespace URSA.Web.Http.Description.Owl
{
    /// <summary>Describes an inverse functional property.</summary>
    [Class("owl", "InverseFunctionalProperty")]
    public interface IInverseFunctionalProperty : Rdfs.IProperty
    {
    }
}