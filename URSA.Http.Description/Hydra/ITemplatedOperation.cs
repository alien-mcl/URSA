using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDeF.Entities;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Describes an operation with a link template.</summary>
    public interface ITemplatedOperation : IEntity
    {
        /// <summary>Gets or sets a link template.</summary>
        IEntity TemplatedLink { get; set; }
    }
}
