using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using URSA.Web;

namespace URSA.Example.WebApplication.Data
{
    /// <summary>Acts as a simple data repository that persists it's data as JSON files.</summary>
    /// <typeparam name="TEntity">Type of the entiti stored.</typeparam>
    /// <typeparam name="TId">Type of the entity identifier.</typeparam>
    public class JsonFilePersistingRepository<TEntity, TId> : IPersistingRepository<TEntity, TId> where TEntity : class, IControlledEntity<TId>
    {
        private static readonly object Lock = new Object();

        private readonly string _rootPath;

        /// <summary>Initializes a new instance of the <see cref="JsonFilePersistingRepository{TEntity, TId}"/> class.</summary>
        /// <param name="rootPath">The root path to store files in.</param>
        public JsonFilePersistingRepository(string rootPath)
        {
            if (rootPath == null)
            {
                throw new ArgumentException("rootPath");
            }

            if (rootPath.Length == 0)
            {
                throw new ArgumentOutOfRangeException("rootPath");
            }

            if (!Directory.Exists(_rootPath = rootPath))
            {
                Directory.CreateDirectory(_rootPath);
            }
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                lock (Lock)
                {
                    return Directory.GetFiles(_rootPath, "*.json").Length;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<TEntity> All(int skip = 0, int take = 0, Func<TEntity, bool> predicate = null)
        {
            var result = new List<TEntity>();
            lock (Lock)
            {
                var serializer = new JsonSerializer();
                IEnumerable<string> files = Directory.GetFiles(_rootPath, "*.json");
                if (skip > 0)
                {
                    files = files.Skip(skip);
                }

                if (take > 0)
                {
                    files = files.Take(take);
                }

                foreach (var filePath in files)
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (var textReader = new StreamReader(fileStream))
                    using (var jsonReader = new JsonTextReader(textReader))
                    {
                        var entity = serializer.Deserialize<TEntity>(jsonReader);
                        if ((predicate == null) || (predicate(entity)))
                        {
                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        /// <inheritdoc />
        public TEntity Get(TId id)
        {
            lock (Lock)
            {
                string filePath = MakeFilePath(id);
                if (!File.Exists(filePath))
                {
                    return null;
                }

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var textReader = new StreamReader(fileStream))
                using (var jsonReader = new JsonTextReader(textReader))
                {
                    return new JsonSerializer().Deserialize<TEntity>(jsonReader);
                }
            }
        }
        
        /// <inheritdoc />
        public void Create(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            string filePath = MakeFilePath(entity.Key);
            lock (Lock)
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                using (var textWriter = new StreamWriter(fileStream))
                {
                    new JsonSerializer().Serialize(textWriter, entity);
                }
            }
        }

        /// <inheritdoc />
        public void Update(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            string filePath = MakeFilePath(entity.Key);
            lock (Lock)
            {
                if (!File.Exists(filePath))
                {
                    throw new NotFoundException(String.Format("{1} with identifier of '{0}' does not exist.", entity.Key, typeof(TEntity).Name));
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                using (var textWriter = new StreamWriter(fileStream))
                {
                    new JsonSerializer().Serialize(textWriter, entity);
                }
            }
        }

        /// <inheritdoc />
        public void Delete(TId id)
        {
            string filePath = MakeFilePath(id);
            lock (Lock)
            {
                if (!File.Exists(filePath))
                {
                    throw new NotFoundException(String.Format("{1} with identifier of '{0}' does not exist.", id, typeof(TEntity).Name));
                }

                File.Delete(filePath);
            }
        }

        private string MakeFilePath(TId id)
        {
            return Path.Combine(_rootPath, id + ".json");
        }
    }
}