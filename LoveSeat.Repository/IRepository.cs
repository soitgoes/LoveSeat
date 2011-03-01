using System;

namespace LoveSeat.Repository
{
    public interface IRepository<T> where T : IBaseObject
    {
        void Save(T item);
        T Find(Guid id);
    }
}