using System;
using System.Threading.Tasks;
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

        public async virtual Task Save(T item)
        {
            if (item.Id == "")
                item.Id = Guid.NewGuid().ToString();
            var doc = new Document<T>(item);
            await db.SaveDocument(doc);
        }

        public async virtual Task<T> Find(Guid id)
        {
            return await db.GetDocument<T>(id.ToString());
        }

        /// <summary>
        /// Repository methods don't have the business validation.  Use the service methods to enforce.
        /// </summary>
        /// <param name="obj"></param>
        public async virtual Task Delete(T obj)
        {
            await db.DeleteDocument(obj.Id, obj.Rev);
        }
    }
}