using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using System.Collections.Generic;

namespace URSA.Web.Http.Description.Owl
{
    /// <summary>Describes an OWL class.</summary>
    [Class("owl", "Class")]
    public interface IClass : Rdfs.IClass
    {
    }
}