using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LoveSeat.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace LoveSeat
{
    public class ViewOptions : IViewOptions
    {
        /// <summary>
        /// Limit the length of the Keys parameter to 6000 characters.
        /// Services such as Cloudant limit the URL length to 8k so 6000 should be on the safe side.
        /// </summary>
        private const int KeysLengthLimit = 6000;

        public ViewOptions()
        {
            Key = new KeyOptions();
            StartKey = new KeyOptions();
            EndKey = new KeyOptions();
        }
       /// <summary>
        /// If you have a complex object as a string set this using a JRaw object()
        /// </summary>
        public IKeyOptions Key { get; set; }
        public IEnumerable<IKeyOptions> Keys { get; set; }
        /// <summary>
        /// If you have a complex object as a string set this using a JRaw object()
        /// </summary>
        public IKeyOptions StartKey { get; set; }
        public string StartKeyDocId { get; set; }
        /// <summary>
        /// If you have a complex object as a string set this using a JRaw object()
        /// </summary>
        public IKeyOptions EndKey { get; set; }
        public string EndKeyDocId { get; set; }
        public int? Limit { get; set; }
        public int? Skip { get; set; }
        public bool? Reduce { get; set; }
        public bool? Group { get; set; }
        public bool? IncludeDocs { get; set; }
        public bool? InclusiveEnd { get; set; }
        public int? GroupLevel { get; set; }
        public bool? Descending { get; set; }
        public bool? Stale { get; set; }
        public string StaleOption { get; set; }
        public string Etag { get; set; }


        public  override string ToString()
        {
            string result = "";
            if ((Key != null) && (Key.Count > 0))
                result += "&key=" + Key.ToString();
            if (Keys != null && !isAtKeysSizeLimit)
              result += "&keys=[" + BuildKeysString() + "]";
            if ((StartKey != null) && (StartKey.Count > 0))
                if((StartKey.Count == 1) && (EndKey.Count > 1))
                    result += "&startkey=[" + StartKey.ToString() + "]";
                else
                    result += "&startkey=" + StartKey.ToString();
            if ((EndKey != null) && (EndKey.Count > 0))
                result += "&endkey=" + EndKey.ToString();
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
            if (Stale.HasValue && Stale.Value)
            {
                if(!string.IsNullOrEmpty(StaleOption))
                {
                    if (StaleOption.ToLower() == "ok")
                        result += "&stale=ok";
                    else if (StaleOption.ToLower() == "update_after")
                        result += "&stale=update_after";
                    else
                        throw new ArgumentException("Invalid StaleOption provided as a CouchDB ViewOption - as of v 1.1.0, valid options include 'ok' and 'update_after'.");
                }
                else
                {
                    result += "&stale=ok";
                }
            }
            if (!string.IsNullOrEmpty(StartKeyDocId))
                result += "&startkey_docid=" + StartKeyDocId;
            if (!string.IsNullOrEmpty(EndKeyDocId))
                result += "&endkey_docid=" + EndKeyDocId;
            return result.Length < 1 ? "" :  "?" + result.Substring(1);
        }

        private string BuildKeysString() {
          return String.Join(",", Keys.Select(k => k.ToString()).ToArray());
        }

        /// <summary>
        /// Get indication if the length of keys parameter went over the allowed limit.
        /// This indicate that the Keys parameter should be encoded in the requeqst body
        /// instead of URL paraemter.
        /// </summary>
        internal bool isAtKeysSizeLimit
        {
          get { return Keys != null && Keys.Any() && BuildKeysString().Length > KeysLengthLimit; }
        }
    }
   

}