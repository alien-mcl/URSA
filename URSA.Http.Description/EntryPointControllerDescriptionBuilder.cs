using System;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Mapping;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a description of the <see cref="EntryPointDescriptionController" />.</summary>
    public class EntryPointControllerDescriptionBuilder : ControllerDescriptionBuilder<EntryPointDescriptionController>
    {
        private readonly Url _entryPoint;

        /// <summary>Initializes a new instance of the <see cref="EntryPointControllerDescriptionBuilder"/> class.</summary>
        /// <param name="entryPoint">The entry point to be described.</param>
        /// <param name="defaultValueRelationSelector">The default value relation selector.</param>
        public EntryPointControllerDescriptionBuilder(Url entryPoint, IDefaultValueRelationSelector defaultValueRelationSelector) : base(defaultValueRelationSelector)
        {
            if (entryPoint == null)
            {
                throw new ArgumentNullException("entryPoint");
            }

            _entryPoint = entryPoint;
        }

        /// <inheritdoc />
        protected override RouteAttribute GetControllerRoute()
        {
            return new RouteAttribute(_entryPoint.ToString());
        }

        /// <inheritdoc />
        protected override ControllerInfo<EntryPointDescriptionController> BuildDescriptorInternal()
        {
            var result = base.BuildDescriptorInternal();
            result.Arguments["entryPoint"] = _entryPoint;
            return result;
        }
    }
}