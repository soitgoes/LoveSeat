using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LoveSeat.Core.Interfaces;

namespace LoveSeat.Core.DocUpdater
{
    public class DesignDoc : IBaseObject
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }
        [JsonProperty("language")]
        public string Language { get; set; }
        [JsonProperty("views")]
        public JObject Views { get; set; }
        public string Id { get; set; }
        public string Rev { get; set; }
        public string Type { get { return "designdoc"; } }
    }
}