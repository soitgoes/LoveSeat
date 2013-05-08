using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoveSeat.Interfaces;

namespace LoveSeat
{
    public class ViewOptions : IViewOptions
    {
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
            var result = new StringBuilder();

            if ((Key != null) && (Key.Count > 0))
                result.AppendFormat("&key={0}", Key.ToString());
            if (Keys != null)
                result.AppendFormat("&keys=[{0}]", String.Join(",", Keys.Select(k => k.ToString()).ToArray()));
            if ((StartKey != null) && (StartKey.Count > 0))
                if((StartKey.Count == 1) && (EndKey.Count > 1))
                    result.AppendFormat("&startkey=[{0}]", StartKey.ToString());
                else
                    result.AppendFormat("&startkey={0}", StartKey.ToString());
            if ((EndKey != null) && (EndKey.Count > 0))
                result.AppendFormat("&endkey={0}", EndKey.ToString());
            if (Limit.HasValue)
                result.AppendFormat("&limit={0}", Limit.Value.ToString());
            if (Skip.HasValue)
                result.AppendFormat("&skip={0}", Skip.Value.ToString());
            if (Reduce.HasValue)
                result.AppendFormat("&reduce={0}", Reduce.Value.ToString().ToLowerInvariant());
            if (Group.HasValue)
                result.AppendFormat("&group={0}", Group.Value.ToString().ToLowerInvariant());
            if (IncludeDocs.HasValue)
                result.AppendFormat("&include_docs={0}", IncludeDocs.Value.ToString().ToLowerInvariant());
            if (InclusiveEnd.HasValue)
                result.AppendFormat("&inclusive_end={0}", InclusiveEnd.Value.ToString().ToLowerInvariant());
            if (GroupLevel.HasValue)
                result.AppendFormat("&group_level={0}", GroupLevel.Value.ToString());
            if (Descending.HasValue)
                result.AppendFormat("&descending={0}", Descending.Value.ToString().ToLowerInvariant());
            if (Stale.HasValue && Stale.Value)
            {
                if(!string.IsNullOrEmpty(StaleOption))
                {
                    if (StaleOption.ToLowerInvariant() == "ok")
                        result.Append("&stale=ok");
                    else if (StaleOption.ToLowerInvariant() == "update_after")
                        result.Append("&stale=update_after");
                    else
                        throw new ArgumentException("Invalid StaleOption provided as a CouchDB ViewOption - as of v 1.1.0, valid options include 'ok' and 'update_after'.");
                }
                else
                {
                    result.Append("&stale=ok");
                }
            }
            if (!string.IsNullOrEmpty(StartKeyDocId))
                result.AppendFormat("&startkey_docid={0}", StartKeyDocId);
            if (!string.IsNullOrEmpty(EndKeyDocId))
                result.AppendFormat("&endkey_docid={0}", EndKeyDocId);

            return result.Length < 1 ? "" :  "?" + result.ToString().Substring(1);
        }
    }
   

}