using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoveSeat.Core
{
    public class Document
    {
        protected JObject _jObject = new JObject();

        protected Document()
        {
        }

        public Document(JObject jobj)

        {
            JToken tmp;
            if (jobj.TryGetValue("id", out tmp))
                Id = tmp.Value<string>();

            if (jobj.TryGetValue("rev", out tmp))
                Rev = tmp.Value<string>();

            if (jobj.TryGetValue("_id", out tmp))
                Id = tmp.Value<string>();

            if (jobj.TryGetValue("_rev", out tmp))
                Rev = tmp.Value<string>();

            _jObject = jobj;
        }

        public Document(string json)
        {
            JObject = JObject.Parse(json);
        }

        [JsonIgnore]
        public string Id
        {
            get
            {
                JToken id;
                return _jObject.TryGetValue("_id", out id) ? id.ToString() : null;
            }
            set { _jObject["_id"] = value; }
        }

        [JsonIgnore]
        public string Rev
        {
            get
            {
                JToken rev;
                return _jObject.TryGetValue("_rev", out rev) ? rev.ToString() : null;
            }
            set { _jObject["_rev"] = value; }
        }

        public string Type { get; private set; }

        public JObject JObject
        {
            get { return _jObject; }
            set { _jObject = value; }
        }

        public bool HasAttachment
        {
            get { return _jObject["_attachments"] != null; }
        }

        public void AddAttachment(string filename, byte[] data)
        {
            var jobj = _jObject.GetValue("_attachments") as JObject ?? new JObject();
            jobj[filename] = new JObject();
            jobj[filename]["data"] = Convert.ToBase64String(data);
            _jObject["_attachments"] = jobj;
        }

        public IEnumerable<string> GetAttachmentNames()
        {
            var attachment = _jObject["_attachments"];
            if (attachment == null)
                return null;

            return attachment.Select(x => x.Value<JProperty>().Name);
        }
    }
}