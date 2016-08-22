using System;
using LoveSeat.Interfaces;

namespace LoveSeat.Repositories
{
    public class CouchRepository<T> : IRepository<T> where T : class,IBaseObject
    {
        protected readonly CouchDatabase db = null;

        public CouchRepository(CouchDatabase db)
        {
            this.db = db;
        }

        public virtual void Save(T item)
        {
            if (item.Id == "")
                item.Id = Guid.NewGuid().ToString();
            var doc = new Document<T>(item);
            db.SaveDocument(doc);
        }

        public virtual Document<T> Find(Guid id)
        {
            return db.GetDocument<T>(id.ToString());
        }

        public virtual Document<T> Find(string id)
        {
            return db.GetDocument<T>(id);
        }

    /// <summary>
        /// Repository methods don't have the business validation.  Use the service methods to enforce.
        /// </summary>
        /// <param name="obj"></param>
        public virtual void Delete(T obj)
        {
            db.DeleteDocument(obj.Id.ToString(), obj.Rev);
        }
    }
}