using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace LoveSeat {
    public class CouchResponse : JObject {
        public CouchResponse(JObject obj) : base(obj)
        {
        }
        public int StatusCode { get; set; }
    }
}
