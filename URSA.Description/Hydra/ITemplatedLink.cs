using RomanticWeb.Mapping.Attributes;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Describes a template link.</summary>
    [Class("hydra", "TemplatedLink")]
    public interface ITemplatedLink : Rdfs.IProperty, IResource
    {
    }
}