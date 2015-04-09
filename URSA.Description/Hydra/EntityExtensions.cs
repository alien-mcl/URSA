using System.Collections.Generic;
using System.Linq;
using RomanticWeb.Entities;
using URSA.Web.Http.Description.Owl;

namespace URSA.Web.Http.Description.Hydra
{
    internal static class EntityExtensions
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

        internal static IEnumerable<Rdfs.IResource> GetUniqueIdentifierType(this IClass @class)
        {
            return (from supportedProperty in @class.SupportedProperties
                    let property = supportedProperty.Property
                    where property.Is(RomanticWeb.Vocabularies.Owl.InverseFunctionalProperty)
                    select property.Range).FirstOrDefault() ?? new Rdfs.IResource[0];
        }
    }
}