using System;
using Newtonsoft.Json;

namespace LoveSeat.Interfaces
{
    public interface IBaseObjectMin
    {
        [JsonProperty("_id")]
        string Id { get; set; }
        [JsonProperty("_rev", NullValueHandling = NullValueHandling.Ignore)]
        string Rev { get; set; }

    }

    public interface IBaseObject : IBaseObjectMin
    {
        string Type { get; }
    }

    public interface IBaseResponse
    {
        [JsonProperty("ok")]
        bool Ok { get; set; }
    }
}
