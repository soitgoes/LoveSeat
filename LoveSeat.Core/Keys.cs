using Newtonsoft.Json;
using System.Collections.Generic;

namespace LoveSeat.Core
{
    /// <summary>
    /// Class containing list of keys for fetching multiple documents with /all_docs 
    /// </summary>
    public class Keys
    {
        public Keys()
        {
            Values = new List<string>();
        }

        [JsonProperty("keys")]
        public List<string> Values { get; set; }

    }
}
