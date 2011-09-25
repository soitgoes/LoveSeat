using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace LoveSeat
{
    public interface IObjectSerializer<T>
    {
        T Deserialize(string json);
        string Serialize(object obj);
    }
    public class ObjectSerializer<T> : IObjectSerializer<T>
    {
        protected readonly JsonSerializerSettings settings;
        public ObjectSerializer()
        {
            settings = new JsonSerializerSettings();
            var converters = new List<JsonConverter> { new IsoDateTimeConverter() };
            settings.Converters = converters;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.NullValueHandling = NullValueHandling.Include;
        }

        public virtual T Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, settings);
        }
        public virtual string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
        }
    }
}