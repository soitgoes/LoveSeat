using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoveSeat.Core.Interfaces
{
    public interface IRepository<T> 
        where T : class, IBaseObject
    {
        void Save(T item);
        Document<T> Find(Guid id);
        Document<T> Find(string id);
        IEnumerable<T> FindAll(ViewOptions options);
        void Delete(T item);

        Task SaveAsync(T item);
        Task<Document<T>> FindAsync(Guid id);
        Task<Document<T>> FindAsync(string id);
        Task<IEnumerable<T>> FindAllAsync(ViewOptions options);
        Task DeleteAsync(T item);
    }
}