using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace LoveSeat
{
    public class ViewOptions
    {
        public JToken Key { get; set; }
        public JToken StartKey { get; set; }
        public JToken EndKey { get; set; }
        public int? Limit { get; set; }
        public int? Skip { get; set; }
        public bool? Reduce { get; set; }
        public bool? Group { get; set; }
        public bool? IncludeDocs { get; set; }
        public bool? InclusiveEnd { get; set; }
        public int? GroupLevel { get; set; }
        public bool? Descending { get; set; }

        public override string ToString()
        {
            string result = "";
            if (Key != null)
                result += "&key=" + Key.ToString(Formatting.None);
            if (StartKey != null)
                result += "&startkey=" + StartKey.ToString(Formatting.None);
            if (EndKey != null)
                result += "&endkey=" + EndKey.ToString(Formatting.None);
            if (Limit.HasValue)
                result += "&limit=" + Limit.Value.ToString();
            if (Skip.HasValue)
                result += "&skip=" + Skip.Value.ToString();
            if (Reduce.HasValue)
                result += "&reduce=" + Reduce.Value.ToString().ToLower();
            if (Group.HasValue)
                result += "&group=" + Group.Value.ToString().ToLower();
            if (IncludeDocs.HasValue)
                result += "&include_docs=" + IncludeDocs.Value.ToString().ToLower();
            if (InclusiveEnd.HasValue)
                result += "&inclusive_end=" + InclusiveEnd.Value.ToString().ToLower();
            if (GroupLevel.HasValue)
                result += "&group_level=" + GroupLevel.Value.ToString();
            if (Descending.HasValue)
                result += "&descending=" + Descending.Value.ToString().ToLower();
            return result.Substring(1);
        }
    }
   

}