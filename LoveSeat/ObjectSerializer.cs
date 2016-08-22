using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using LoveSeat;

namespace LoveSeat
{
    public interface IObjectSerializer
    {
        T Deserialize<T>(string json) where T: class;
        string Serialize<T>(T obj) where T : class;
        Document<T> DeserializeToDoc<T>(string json) where T : class;
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

        public bool DefaultValueIfFailedSerialization = true;

        public Document<T> DeserializeToDoc<T>(string json) where T: class
        {
            return new Document<T>(json);
        }

        public T Deserialize<T>(string json) where T: class
        {
            if (!DefaultValueIfFailedSerialization)
                return JsonConvert.DeserializeObject<T>(json, settings);
            try
            {
                return JsonConvert.DeserializeObject<T>(json, settings);
            }
            catch (Exception ex)
            {
                //TODO: Build Logger Property and record exception   
                Console.WriteLine(ex);
            }
            return default(T);
        }

        public string Serialize<T>(T obj) where T: class
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
        }
    }
}