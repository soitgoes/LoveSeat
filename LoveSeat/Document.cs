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

    #region Bulk documents

    /// <summary>
    /// Class containing list of documents for bulk loading multiple documents with /all_docs
    /// </summary>
    public class Documents
    {
        public Documents()
        {
            Values = new List<Document>();
        }

        [JsonProperty("docs")]
        public List<Document> Values { get; set; }
    }

    /// <summary>
    /// Class containing list of keys for fetching multiple documents with /all_docs 
    /// </summary>
    public class Keys
    {
        public Keys()
        {
            Values = new List<string>();
        }

        [JsonProperty("keys")]
        public List<string> Values { get; set; }

    }

    public class BulkDocumentResponses : List<BulkDocumentResponse>
    { }

    public class BulkDocumentResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("rev")]
        public string Rev { get; set; }

        [JsonProperty("error")]
        public string Error {get;set;}

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }

    #endregion



    public class Document : JObject, IBaseObject
    {
        [JsonIgnore]
        public string Id
        {
            get { 
                JToken id;
                return this.TryGetValue("_id", out id) ? id.ToString() : null;
            } 
            set { this["_id"] = value; }
        }

        [JsonIgnore]
        public string Rev { 
            get { 
                JToken rev;
                return this.TryGetValue("_rev", out rev) ? rev.ToString() : null;
            }
            set { this["_rev"] = value; }
        }

        public new string Type { get; private set; }

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