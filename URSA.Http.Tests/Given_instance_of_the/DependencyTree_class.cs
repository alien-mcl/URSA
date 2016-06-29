using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web;
using URSA.Web.Http.Collections;

namespace Given_instance_of_the
{
    [TestClass]
    public class DependencyTree_class
    {
        private IResponseModelTransformer[] _expected;
        private DependencyTree<IResponseModelTransformer> _dependencyTree;

        [TestMethod]
        public void it_should_enumerate_model_transformers_in_a_correct_order()
        {
            int index = 0;
            foreach (var modelTransformer in _dependencyTree)
            {
                modelTransformer.Should().Be(_expected[index]);
                index++;
            }
        }

        [TestInitialize]
        public void Setup()
        {
            var modelTransformers = new IResponseModelTransformer[]
                {
                    new YetAnotherDependentyResponseModelTransformer(),
                    new SomeAnotherDependentyResponseModelTransformer(),
                    new SomeDependentyResponseModelTransformer(),
                    new SomeResponseModelTransformer()
                };
            _dependencyTree = new DependencyTree<IResponseModelTransformer>(modelTransformers, typeof(IResponseModelTransformer<>));
            _expected = new[]
                {
                    modelTransformers[3],
                    modelTransformers[2],
                    modelTransformers[0],
                    modelTransformers[1]
                };
        }

        [TestCleanup]
        public void Teardown()
        {
            _dependencyTree = null;
        }

        private class SomeResponseModelTransformer : IResponseModelTransformer
        {
            public Task<object> Transform(IRequestMapping requestMapping, IRequestInfo request, object result, object[] arguments)
            {
                return Task.FromResult(result);
            }
        }

        private class SomeDependentyResponseModelTransformer : IResponseModelTransformer<SomeResponseModelTransformer>
        {
            public Task<object> Transform(IRequestMapping requestMapping, IRequestInfo request, object result, object[] arguments)
            {
                return Task.FromResult(result);
            }
        }

        private class SomeAnotherDependentyResponseModelTransformer : IResponseModelTransformer<SomeDependentyResponseModelTransformer>
        {
            public Task<object> Transform(IRequestMapping requestMapping, IRequestInfo request, object result, object[] arguments)
            {
                return Task.FromResult(result);
            }
        }

        private class YetAnotherDependentyResponseModelTransformer : IResponseModelTransformer<SomeDependentyResponseModelTransformer>
        {
            public Task<object> Transform(IRequestMapping requestMapping, IRequestInfo request, object result, object[] arguments)
            {
                return Task.FromResult(result);
            }
        }
    }
}