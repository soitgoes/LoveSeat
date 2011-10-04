using System;
using LoveSeat.Interfaces;

namespace LoveSeat.Interfaces
{
    public interface IRepository<T> where T : IBaseObject
    {
        void Save(T item);
        T Find(Guid id);
    }
}