using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
   public class KeyOptions : JArray
    {
       
       public override string ToString()
       {
           if (Count == 1)
           {
               return this[0].ToString(Formatting.None, new IsoDateTimeConverter());
           }
           if (Count > 1)
           {
               return base.ToString(Formatting.None, new IsoDateTimeConverter());    
           }
           return "";           
       }
       
    }
}
