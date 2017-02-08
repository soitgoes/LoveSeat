using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using LoveSeat.Core.Interfaces;

namespace LoveSeat.Core
{
    public class DefaultSerializer : IObjectSerializer
    {
        protected readonly JsonSerializerSettings _settings;

        public DefaultSerializer()
        {
            _settings = new JsonSerializerSettings();
            var converters = new List<JsonConverter> { new IsoDateTimeConverter() };

            _settings.Converters = converters;
            _settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            _settings.NullValueHandling = NullValueHandling.Ignore;
        }

        public bool DefaultValueIfFailedSerialization = true;

        public Document<T> DeserializeToDoc<T>(string json) 
            where T : class
        {
            return new Document<T>(json);
        }

        public T Deserialize<T>(string json) where T : class
        {
            if (!DefaultValueIfFailedSerialization)
                return JsonConvert.DeserializeObject<T>(json, _settings);

            try
            {
                return JsonConvert.DeserializeObject<T>(json, _settings);
            }
            catch (Exception ex)
            {
                //TODO: Build Logger Property and record exception   
                Console.WriteLine(ex);
            }

            return default(T);
        }

        public string Serialize<T>(T obj) where T : class
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, _settings);
        }
    }
}