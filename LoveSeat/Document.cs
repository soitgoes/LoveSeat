using System;
using System.Collections.Generic;
using System.Linq;
using LoveSeat.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
    public class Document<T> : Document
    {
        private static IObjectSerializer objectSerializer = new DefaultSerializer();

        public Document(T obj) : base(objectSerializer.Serialize<T>(obj)) {
            }
        public Document(T obj, IObjectSerializer objectSerializer) : base(objectSerializer.Serialize<T>(obj))
        {            
        }
    }

    

    public class Document : JObject, IBaseObject
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("_rev")]
        public string Rev { get; set; }

        public string Type { get; private set; }

        protected Document()
        {
        }
        public Document(JObject jobj)
            : base(jobj)
        {
            JToken tmp;
            if (jobj.TryGetValue("id", out tmp))
                this.Id = tmp.Value<string>();
            if (jobj.TryGetValue("rev", out tmp))
                this.Rev = tmp.Value<string>();
            if (jobj.TryGetValue("_id", out tmp))
                this.Id = tmp.Value<string>();
            if (jobj.TryGetValue("_rev", out tmp))
                this.Rev = tmp.Value<string>();
        }
        public Document(string json)
            : base(JObject.Parse(json))
        {
        }
        public bool HasAttachment
        {
            get { return this["_attachments"] != null; }
        }

        public IEnumerable<string> GetAttachmentNames()
        {
            var attachment = this["_attachments"];
            if (attachment == null) return null;
            return attachment.Select(x => x.Value<JProperty>().Name);
        }

    }
}