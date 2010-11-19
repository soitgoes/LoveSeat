using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
    public class ViewResult : JObject
    {
        public ViewResult(JObject obj)
            : base(obj)
        {
        }
        public int TotalRows { get { return this["total_rows"].Value<int>(); } }
        public int OffSet { get { return this["offset"].Value<int>(); } }
        public IEnumerable<JToken> Rows { get { return (JArray)this["rows"]; } }
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
    }
}