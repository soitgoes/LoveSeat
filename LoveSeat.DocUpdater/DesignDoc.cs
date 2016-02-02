using System.Collections.Generic;
using LoveSeat.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoveSeat.DocUpdater
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

        public string Type
        {
            get { return "designdoc"; }
        }
    }
}