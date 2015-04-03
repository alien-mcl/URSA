using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RomanticWeb.Entities;
using URSA.Web.Http.Description.Owl;

namespace URSA.Web.Http.Description.Hydra
{
    internal static class ClassExtensions
    {
        internal static bool IsGenericRdfList(this IClass @class, out IResource itemRestriction)
        {
            bool result = false;
            itemRestriction = null;
            foreach (var superClass in @class.SubClassOf)
            {
                if (superClass.Is(RomanticWeb.Vocabularies.Rdf.List))
                {
                    result = true;
                }

                IRestriction restriction = null;
                if ((superClass.Is(RomanticWeb.Vocabularies.Owl.Restriction)) && 
                    ((restriction = superClass.AsEntity<IRestriction>()).OnProperty != null) && (restriction.OnProperty.Id == RomanticWeb.Vocabularies.Rdf.first) && 
                    (restriction.AllValuesFrom != null) && (restriction.AllValuesFrom.Is(RomanticWeb.Vocabularies.Rdfs.Resource)))
                {
                    itemRestriction = restriction.AllValuesFrom.AsEntity<IResource>();
                }
            }

            return result;
        }
    }
}