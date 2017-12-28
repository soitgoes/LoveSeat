using System;
using System.Collections.Generic;
using System.Linq;
using LoveSeat;
using LoveSeat.Interfaces;
using LoveSeat.Support;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection.Emit;
namespace LoveSeat
{
    public class Document<T> : Document where T : class
    {
        private static IObjectSerializer objectSerializer = new DefaultSerializer();

        public Document(T obj) : base(objectSerializer.Serialize<T>(obj)) {
            }
        public Document(T obj, IObjectSerializer objectSerializer) : base(objectSerializer.Serialize<T>(obj))
        {            
        }
        public Document(string json) : base(json)
        {
            
        }

        public void UpdateFromItem(T item)
        {
            jObject.CopyFromObj(item);
        }

        public T Item { 
            get
            {
                return jObject.ToObject<T>();
                 //var moqObject = new Moq.Mock<T>(MockBehavior.Loose);
                 //var properties = typeof (T).GetProperties();
                 //var tItem = objectSerializer.Deserialize<T>(jObject.ToString()); //So that it adhere's to the Strategy
                 //foreach (var prop in properties)
                 //{

                 //    var setMethod = prop.GetSetMethod();
                 //    var method = setMethod.GetMethodBody();

                 //    moqObject.SetupGet(X => ).Returns(prop.GetValue(tItem));

                 //    moqObject.SetupSet<T>(x =>  x).Callback(x => x.)
                 //}

                 //return moqObject.Object;
            } 
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



    public class Document
    {

        protected JObject jObject = new JObject();
        [JsonIgnore]
        public string Id
        {
            get { 
                JToken id;
                return jObject.TryGetValue("_id", out id) ? id.ToString() : null;
            } 
            set { jObject["_id"] = value; }
        }

        [JsonIgnore]
        public string Rev { 
            get { 
                JToken rev;
                return jObject.TryGetValue("_rev", out rev) ? rev.ToString() : null;
            }
            set { jObject["_rev"] = value; }
        }

        public string Type { get; private set; }

        public JObject JObject { get { return jObject; } set { jObject = value; } }

        public Document()
        {
        }
        public Document(JObject jobj)
            
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
            this.jObject = jobj;
        }
        public Document(string json)
        {
            JObject = JObject.Parse(json);
        }
        public bool HasAttachment
        {
            get { return jObject["_attachments"] != null; }
        }

        public void AddAttachment(string filename, byte[] data)
        {
            var jobj = jObject.GetValue("_attachments") as JObject ?? new JObject();
            jobj[filename] = new JObject();
            jobj[filename]["data"] = Convert.ToBase64String(data);
            jObject["_attachments"] = jobj;
        }

        public IEnumerable<string> GetAttachmentNames()
        {
            var attachment = jObject["_attachments"];
            if (attachment == null) return null;
            return attachment.Select(x => x.Value<JProperty>().Name);
        }

    }
}