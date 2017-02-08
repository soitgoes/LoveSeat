using System.Collections.Generic;
using LoveSeat.Core.Support;

namespace LoveSeat.Core.Interfaces
{
    public interface IViewResult<T> : IViewResult
    {
        CouchDictionary<T> Dictionary { get; }
        IEnumerable<T> Items { get; }
    }
}