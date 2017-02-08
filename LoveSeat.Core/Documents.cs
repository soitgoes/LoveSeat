using Newtonsoft.Json;
using System.Collections.Generic;

namespace LoveSeat.Core
{
    /// <summary>
    /// Class containing list of documents for bulk loading multiple documents with /all_docs
    /// </summary>
    public class Documents
    {
        public Documents()
        {
            Values = new List<Document>();
        }

        [JsonProperty("docs")]
        public List<Document> Values { get; set; }
    }
}
