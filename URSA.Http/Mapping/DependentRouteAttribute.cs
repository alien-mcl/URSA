using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URSA.Web.Mapping;

namespace URSA.Web.Mapping
{
    /// <summary>Binds the target of the attribute to the related controller.</summary>
    public class DependentRouteAttribute : RouteAttribute
    {
        /// <summary>Initializes a new instance of the <see cref="DependentRouteAttribute"/> class.</summary>
        public DependentRouteAttribute() : base("/")
        {
        }
    }
}