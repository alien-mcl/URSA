using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Wraps a data type as an <![CDATA[rdfs:Class]]>.</summary>
    /// <remarks>This is only to address limitation of <![CDATA[HyDrA]]> of expecting classes only.</remarks>
    [Class("ursa", "DatatypeDefinition")]
    public interface IDatatypeDefinition : Rdfs.IClass
    {
        /// <summary>Gets or sets the data type defined.</summary>
        [Property("ursa", "defines")]
        IEntity Datatype { get; set; }
    }
}