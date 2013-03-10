using System;
using System.Threading.Tasks;
using LoveSeat.Interfaces;

namespace LoveSeat.Interfaces
{
    public interface IRepository<T> where T : IBaseObject
    {
        Task Save(T item);
        Task<T> Find(Guid id);
    }
}