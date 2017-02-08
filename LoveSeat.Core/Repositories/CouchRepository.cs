using LoveSeat.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoveSeat.Core.Repositories
{
    public class CouchRepository<T> : IRepository<T> where T : class, IBaseObject
    {
        protected readonly CouchDatabase _db = null;

        public CouchRepository(CouchDatabase db)
        {
            _db = db;
        }

        public virtual void Save(T item)
        {
            SaveAsync(item).GetAwaiter().GetResult();
        }

        public virtual Document<T> Find(Guid id)
        {
            return FindAsync(id).GetAwaiter().GetResult();
        }

        public virtual Document<T> Find(string id)
        {
            return FindAsync(id).GetAwaiter().GetResult();
        }

        public virtual IEnumerable<T> FindAll(ViewOptions options = null)
        {
            return FindAllAsync(options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Repository methods don't have the business validation.
        /// Use the service methods to enforce.
        /// </summary>
        /// <param name="item"></param>
        public virtual void Delete(T item)
        {
            DeleteAsync(item).GetAwaiter().GetResult();
        }

        public async Task SaveAsync(T item)
        {
            if (item.Id == "")
                item.Id = Guid.NewGuid().ToString();

            var doc = new Document<T>(item);
            await _db.SaveDocumentAsync(doc).ConfigureAwait(false);
        }

        public Task<Document<T>> FindAsync(Guid id)
        {
            return _db.GetDocumentAsync<T>(id.ToString());
        }

        public Task<Document<T>> FindAsync(string id)
        {
            return _db.GetDocumentAsync<T>(id);
        }

        public async Task<IEnumerable<T>> FindAllAsync(ViewOptions options)
        {
            options = options ?? new ViewOptions() { IncludeDocs = true };

            List<T> resultDocs = new List<T>();

            var docs = await _db.GetAllDocumentsAsync(options).ConfigureAwait(false);

            foreach (var doc in docs.Docs)
            {
                resultDocs.Add(doc.ToObject<T>());
            }

            return resultDocs;
        }

        public async Task DeleteAsync(T item)
        {
            await _db.DeleteDocumentAsync(item.Id.ToString(), item.Rev).ConfigureAwait(false);
        }
    }
}
