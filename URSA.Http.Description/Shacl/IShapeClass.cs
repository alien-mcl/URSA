using System;
using System.Collections.Generic;
using RomanticWeb;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using RomanticWeb.Vocabularies;
using URSA.Web.Http.Description.Rdfs;

namespace URSA.Web.Http.Description.Shacl
{
    /// <summary>Describes a class shape.</summary>
    [Class("shacl", "ShapeClass")]
    public interface IShapeClass : IClass, IShape
    {
    }
}