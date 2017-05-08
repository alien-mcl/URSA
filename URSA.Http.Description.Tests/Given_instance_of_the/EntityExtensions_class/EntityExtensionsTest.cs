using System;
using URSA.Configuration;
using URSA.Web.Http.Description.Tests.Data;
using System.Linq;
using NUnit.Framework;
using RDeF.Entities;

namespace Given_instance_of_the.EntityExtensions_class
{
    [TestFixture]
    public abstract class EntityExtensionsTest
    {
        protected ISerializableEntitySource SourceEntitySource { get; private set; }

        protected ISerializableEntitySource TargetEntitySource { get; private set; }

        protected IEntityContext SourceEntityContext { get; private set; }

        protected IEntityContext TargetEntityContext { get; private set; }

        protected IProduct SourceInstance { get; private set; }

        protected IProduct TargetInstance { get; set; }

        [SetUp]
        public virtual void Setup()
        {
            ISerializableEntitySource entitySource;
            SourceEntityContext = SetupEntityContext(out entitySource);
            SourceEntitySource = entitySource;
            TargetEntityContext = SetupEntityContext(out entitySource);
            TargetEntitySource = entitySource;
            SourceInstance = SourceEntityContext.Create<IProduct>(new Iri("http://temp.uri/product/"));
            SourceInstance.Name = "Test";
            SourceInstance.Price = 0;
            SourceInstance.Key = Guid.NewGuid();
            SourceInstance.RelatedProduct = SourceEntityContext.Create<IProduct>(new Iri("http://temp.uri/related-product/"));
            SourceInstance.Similar.Add(SourceEntityContext.Create<IProduct>(new Iri("http://temp.uri/similar-product/")));
            SourceEntityContext.Commit();
        }

        private IEntityContext SetupEntityContext(out ISerializableEntitySource entitySource)
        {
            var result = new DefaultEntityContextFactory()
                .WithMappings(load => load.FromAssemblyOf<IProduct>())
                .Create();
            entitySource = (ISerializableEntitySource)result.EntitySource;
            return result;
        }
    }
}