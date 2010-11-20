using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
    public class ViewResult : JObject
    {
        public ViewResult(JObject obj)
            : base(obj)
        {
        }
        public string Etag { get; set; }
        public int TotalRows { get { return this["total_rows"].Value<int>(); } }
        public int OffSet { get { return this["offset"].Value<int>(); } }
        public IEnumerable<JToken> Rows { get { return (JArray)this["rows"]; } }
        /// <summary>
        /// Only populated when IncludeDocs is true
        /// </summary>
        public IEnumerable<JToken> Docs { get { return (JArray) this["doc"]; } }
        /// <summary>
        /// An IEnumerable of strings insteda of the IEnumerable of JTokens
        /// </summary>
        public IEnumerable<string> RawRows
        {
            get
            {
                var arry = (JArray)this["rows"];
                return arry.Select(item => item.ToString());
            }
        }

        public IEnumerable<string> RawValues
        {
            get { var arry = (JArray) this["rows"];
                return arry.Select(item => item["value"].ToString());
            }
        }
        public IEnumerable<string> RawDocs
        {
            get
            {
                var arry = (JArray)this["rows"];
                return arry.Select(item => item["doc"].ToString());
            }
        }
        public string RawString
        {
            get { return this.ToString(); }
        }
    }
}