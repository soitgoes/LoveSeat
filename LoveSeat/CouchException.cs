using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoveSeat
{
   public  class CouchException : System.Exception
    {
       public CouchException(string mesg) : base(mesg)
       {
       }
    }
}
