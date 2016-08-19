using System;
using Accounting.Domain;
using LoveSeat.Interfaces;

namespace LoveSeat.Repositories
{
    public abstract class AuditableRepository<T> : CouchRepository<T> where T : IAuditableRecord
    {
        protected AuditableRepository(CouchDatabase db) : base(db)
        {
        }
        public  override void Save(T item)
        {
            if (item.Rev == null)
                item.CreatedAt = DateTime.Now;
            item.LastModifiedAt = DateTime.Now;
            if (item.Id == string.Empty)
                item.Id = Guid.NewGuid().ToString();    
            base.Save(item);
        }
    }
}