using System;
using LoveSeat.Interfaces;

namespace LoveSeat.Repositories
{
    public class CouchRepository<T> : IRepository<T> where T : IBaseObject
    {
        protected readonly CouchDatabase db = null;
        public  CouchRepository(CouchDatabase db)
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

        public virtual T Find(Guid id)
        {
            return db.GetDocument<T>(id.ToString());
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