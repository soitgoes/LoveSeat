using LoveSeat.Core.Interfaces;
using System;

namespace LoveSeat.Core.Repositories
{
    public abstract class AuditableRepository<T> : CouchRepository<T> where T : class, IAuditableRecord
    {
        protected AuditableRepository(CouchDatabase db) 
            : base(db)
        {
        }

        public override void Save(T item)
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
