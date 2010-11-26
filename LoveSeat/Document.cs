using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
    public class Document<T> : Document
    {
        private static IObjectSerializer<T> objectSerializer = new ObjectSerializer<T>();

        public Document(T obj) : base(objectSerializer.Serialize(obj)) {
            }
        public Document(T obj, IObjectSerializer<T> objectSerializer) : base(objectSerializer.Serialize(obj))
        {            
        }
    }

    public class Document : JObject
    {
        public string Id
        {
            get { return this["_id"].Value<string>(); }
            set { this["_id"] = value; }
        }
        public string Rev
        {
            get
            {
                JToken  rev;
                if (this.TryGetValue("_rev",out rev ))
                {
                    return rev.Value<string>();
                }
                return null;
            }
            set { this["_rev"] = value; }
        }
        protected Document()
        {
        }
        public Document(JObject jobj)
            : base(jobj)
        {
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