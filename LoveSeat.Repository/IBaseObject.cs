using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoveSeat.Repository
{
    public interface IBaseObject
    {
        Guid Id { get; set; }
        string Rev { get; set; }
        string Type { get; }
    }
}
