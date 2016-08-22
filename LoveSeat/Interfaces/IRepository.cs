using System;
using LoveSeat.Interfaces;
using LoveSeat;

namespace LoveSeat.Interfaces
{
    public interface IRepository<T> where T : class, IBaseObject
    {
        void Save(T item);
        Document<T> Find(Guid id);
        Document<T> Find(string id);
    }
}