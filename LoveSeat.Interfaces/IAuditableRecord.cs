using System;

namespace LoveSeat.Interfaces
{
    public interface IAuditableRecord : IBaseObject
    {
        DateTime CreatedAt { get; set; }
        DateTime LastModifiedAt { get; set; }
        string CreatedBy { get; set; }
        string LastModifiedBy { get; set; }
    }
}