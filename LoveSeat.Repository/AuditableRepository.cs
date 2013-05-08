using System;
using System.Threading.Tasks;
using Accounting.Domain;
using LoveSeat.Interfaces;

namespace LoveSeat.Repositories
{
    public abstract class AuditableRepository<T> : CouchRepository<T> where T : IAuditableRecord
    {
        protected AuditableRepository(CouchDatabase db) : base(db)
        {
        }
        public async override Task Save(T item)
        {
            if (item.Rev == null)
                item.CreatedAt = DateTime.Now;
            item.LastModifiedAt = DateTime.Now;
            if (item.Id == string.Empty)
                item.Id = Guid.NewGuid().ToString();
            await base.Save(item).ConfigureAwait(false);
        }
    }
}