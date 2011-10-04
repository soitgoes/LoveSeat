using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace LoveSeat
{
    public interface IObjectSerializer
    {
        T Deserialize<T>(string json);
        string Serialize<T>(T obj);
    }   
    public class DefaultSerializer : IObjectSerializer
    {
        protected readonly JsonSerializerSettings settings;
        public DefaultSerializer() {
            settings = new JsonSerializerSettings();
            var converters = new List<JsonConverter> { new IsoDateTimeConverter() };
            settings.Converters = converters;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.NullValueHandling = NullValueHandling.Ignore;
        }

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
        }
    }
}