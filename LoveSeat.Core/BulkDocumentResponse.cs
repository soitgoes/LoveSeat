using Newtonsoft.Json;

namespace LoveSeat.Core
{
    public class BulkDocumentResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("rev")]
        public string Rev { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}
