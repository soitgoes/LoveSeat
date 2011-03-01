using System;

namespace LoveSeat.Repository
{
    public interface IBaseObject
    {
        Guid Id { get; set; }
        string Rev { get; set; }
        string Type { get; }
    }
}
