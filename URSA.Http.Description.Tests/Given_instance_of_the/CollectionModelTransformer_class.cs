using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RomanticWeb;
using RomanticWeb.Entities;
using RomanticWeb.NamedGraphs;
using URSA.Security;
using URSA.Web;
using URSA.Web.Http;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.NamedGraphs;
using URSA.Web.Http.Description.Tests;
using URSA.Web.Http.Description.Tests.Data;
using URSA.Web.Http.Tests.Testing;

namespace Given_instance_of_the
{
    [TestClass]
    public class CollectionModelTransformer_class
    {
        private static readonly Uri RequestUri = new Uri("http://temp.uri/");
        private static readonly RequestInfo Request = new RequestInfo(Verb.GET, RequestUri, new MemoryStream(), new BasicClaimBasedIdentity(), new Header("Accept", "*/*"));

        private Mock<IRequestMapping> _mapping;
        private Mock<IEntityContext> _entityContext;
        private IModelTransformer _modelTransformer;

        [TestMethod]
        public async Task should_inject_hydra_Collection_details()
        {
            var result = new List<IProduct>() { new Mock<IProduct>(MockBehavior.Strict).Object };
            var arguments = new[] { 1, 0, 0, (object)null };
            var collection = SetupCollection(result.Count);
            _entityContext.Setup(instance => instance.Load<ICollection>(RequestUri)).Returns(collection.Object);

            await _modelTransformer.Transform(_mapping.Object, Request, result, arguments);

            _entityContext.Verify(instance => instance.Load<ICollection>(RequestUri), Times.Once);
            collection.Object.Members.Should().HaveCount(result.Count);
        }

        [TestMethod]
        public async Task should_inject_hydra_PartialCollectionView_details()
        {
            var result = Enumerable.Range(0, 20).Select(index => new Mock<IProduct>(MockBehavior.Strict).Object).ToList();
            var arguments = new[] { 20, 0, 10, (object)null };
            var view = new Mock<IPartialCollectionView>(MockBehavior.Strict);
            var collection = SetupCollection(result.Count, 10, view);
            _entityContext.Setup(instance => instance.Load<ICollection>(RequestUri)).Returns(collection.Object);
            _entityContext.Setup(instance => instance.Load<IPartialCollectionView>(It.IsAny<EntityId>())).Returns(view.Object);

            await _modelTransformer.Transform(_mapping.Object, Request, result, arguments);

            _entityContext.Verify(instance => instance.Load<ICollection>(RequestUri), Times.Once);
            collection.Object.Members.Should().HaveCount(result.Count);
        }

        [TestInitialize]
        public void Setup()
        {
            _mapping = new Mock<IRequestMapping>(MockBehavior.Strict);
            _mapping.SetupGet(instance => instance.Operation).Returns(typeof(TestController).GetMethod("List").ToOperationInfo("/", Verb.GET));
            _mapping.SetupGet(instance => instance.Target).Returns(new TestController());
            _entityContext = new Mock<IEntityContext>(MockBehavior.Strict);
            var blankNodeIdGenerator = new Mock<IBlankNodeIdGenerator>(MockBehavior.Strict);
            blankNodeIdGenerator.Setup(instance => instance.Generate()).Returns("bnode001");
            _entityContext.SetupGet(instance => instance.BlankIdGenerator).Returns(blankNodeIdGenerator.Object);
            _entityContext.Setup(instance => instance.Commit());
            var entityContextProvider = new Mock<IEntityContextProvider>(MockBehavior.Strict);
            entityContextProvider.SetupGet(instance => instance.EntityContext).Returns(_entityContext.Object);
            var namedGraphSelector = new Mock<ILocallyControlledNamedGraphSelector>(MockBehavior.Strict);
            namedGraphSelector.Setup(instance => instance.MapEntityGraphForRequest(It.IsAny<IRequestInfo>(), It.IsAny<EntityId>(), It.IsAny<Uri>()));
            namedGraphSelector.Setup(instance => instance.UnmapEntityGraphForRequest(It.IsAny<IRequestInfo>(), It.IsAny<EntityId>()));
            var namedGraphSelectorFactory = new Mock<INamedGraphSelectorFactory>(MockBehavior.Strict);
            namedGraphSelectorFactory.Setup(instance => instance.NamedGraphSelector).Returns(namedGraphSelector.Object);
            _modelTransformer = new CollectionModelTransformer(entityContextProvider.Object, namedGraphSelectorFactory.Object);
        }

        [TestCleanup]
        public void Teardown()
        {
            _modelTransformer = null;
            _mapping = null;
            _entityContext = null;
        }

        private Mock<ICollection> SetupCollection(int totalItems, int take = 0, Mock<IPartialCollectionView> view = null)
        {
            var collection = new Mock<ICollection>(MockBehavior.Strict);
            collection.SetupGet(instance => instance.Id).Returns(new EntityId(RequestUri));
            collection.SetupSet(instance => instance.TotalItems = totalItems);
            var members = new List<IResource>();
            collection.SetupGet(instance => instance.Members).Returns(members);
            if (view == null)
            {
                return collection;
            }

            collection.SetupSet(instance => instance.View = view.Object);
            collection.SetupGet(instance => instance.Context).Returns(_entityContext.Object);
            view.SetupSet(instance => instance.ItemsPerPage = take);
            return collection;
        }
    }
}