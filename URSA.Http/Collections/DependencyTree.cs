using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace URSA.Web.Http.Collections
{
    internal class DependencyTree<T> : IEnumerable<T>
    {
        private readonly IList<DependencyNode> _dependencies;

        internal DependencyTree(IEnumerable<T> modelTransformers, Type dependencyTypeFlag)
        {
            _dependencies = new List<DependencyNode>();
            var visited = new List<T>();
            foreach (var modelTransformer in modelTransformers)
            {
                if (visited.Contains(modelTransformer))
                {
                    continue;
                }

                _dependencies.Add(new DependencyNode(modelTransformers, modelTransformer, visited, dependencyTypeFlag));
            }
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return new DependencyTreeEnumerator(_dependencies);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class DependencyTreeEnumerator : IEnumerator<T>
        {
            private readonly IList<DependencyNode> _dependencies;
            private readonly IList<T> _visited;

            internal DependencyTreeEnumerator(IList<DependencyNode> dependencies)
            {
                _dependencies = dependencies;
                _visited = new List<T>();
            }

            /// <inheritdoc />
            public T Current { get; private set; }

            /// <inheritdoc />
            object IEnumerator.Current { get { return Current; } }

            /// <inheritdoc />
            public bool MoveNext()
            {
                var result = FindNext(_dependencies, null);
                if (result == null)
                {
                    return false;
                }

                _visited.Add(Current = result.ModelTransformer);
                return true;
            }

            /// <inheritdoc />
            public void Dispose()
            {
            }

            /// <inheritdoc />
            public void Reset()
            {
                _visited.Clear();
            }

            private DependencyNode FindNext(IList<DependencyNode> dependencies, DependencyNode current)
            {
                foreach (var dependency in dependencies)
                {
                    if ((current != null) && (_visited.Contains(current.ModelTransformer)))
                    {
                        continue;
                    }

                    if (dependency.Dependencies.Count > 0)
                    {
                        DependencyNode result = FindNext(dependency.Dependencies, dependency);
                        if (result != null)
                        {
                            return result;
                        }
                    }

                    if (!_visited.Contains(dependency.ModelTransformer))
                    {
                        return dependency;
                    }
                }

                return null;
            }
        }

        private class DependencyNode
        {
            internal DependencyNode(IEnumerable<T> modelTransformers, T modelTransformer, IList<T> visited, Type dependencyTypeFlag)
            {
                ModelTransformer = modelTransformer;
                Dependencies = new List<DependencyNode>();
                var dependencies = from @interface in modelTransformer.GetType().GetTypeInfo().GetInterfaces()
                                   where (@interface.GetTypeInfo().IsGenericType) && (@interface.GetGenericTypeDefinition() == dependencyTypeFlag)
                                   select @interface;
                foreach (var dependency in dependencies)
                {
                    var transformer = (from dependencyTransformer in modelTransformers
                                       where dependency.GetGenericArguments()[0].IsInstanceOfType(dependencyTransformer)
                                       select dependencyTransformer).FirstOrDefault();
                    if ((transformer == null) || (visited.Contains(transformer)))
                    {
                        continue;
                    }

                    visited.Add(transformer);
                    Dependencies.Add(new DependencyNode(modelTransformers, transformer, visited, dependencyTypeFlag));
                }
            }

            internal IList<DependencyNode> Dependencies { get; private set; }

            internal T ModelTransformer { get; private set; }
        }
    }
}