using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using LoveSeat.Interfaces;
using LoveSeat.Support;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
    public class ViewResult<T> : ViewResult, IViewResult<T>
    {
        private readonly IObjectSerializer objectSerializer = null;
        private CouchDictionary<T> dict = null;
        public ViewResult(CouchResponse response, HttpWebRequest request, IObjectSerializer objectSerializer, bool includeDocs = false)
            : base(response, request, includeDocs)
        {
            this.objectSerializer = objectSerializer;

        }

        public CouchDictionary<T> Dictionary
        {
            get
            {
                if (dict != null) return dict;
                dict = new CouchDictionary<T>();
                foreach (var row in this.Rows)
                {
                    dict.Add(row.Value<JToken>("key").ToString(Formatting.None), objectSerializer.Deserialize<T>(row.Value<string>("value")));
                }
                return dict;
            }
        }

        public IEnumerable<T> Items
        {
            get
            {
                if (objectSerializer == null)
                {
                    throw new InvalidOperationException("ObjectSerializer must be set in order to use the generic view.");
                }

                var values = this.IncludeDocs ? this.RawDocs : this.RawValues;
                return values.Select(item => objectSerializer.Deserialize<T>(item));
            }
        }
    }

    public class ViewResult : IViewResult
    {
        private readonly CouchResponse response;
        private readonly HttpWebRequest request;
        private JObject json = null;

        public JObject Json { get { return json ?? (json = JObject.Parse(response.ResponseString)); } }
        public ViewResult(CouchResponse response, HttpWebRequest request, bool includeDocs = false)
        {
            this.response = response;
            this.request = request;
            this.IncludeDocs = includeDocs;
        }
        /// <summary>
        /// Typically won't be needed.  Provided for debuging assistance
        /// </summary>
        public HttpWebRequest Request { get { return request; } }

        public HttpStatusCode StatusCode { get { return response.StatusCode; } }

        public string Etag { get { return response.ETag; } }
        public int TotalRows
        {
            get
            {
                if (Json["total_rows"] == null) throw new CouchException(request, response, Json["reason"].Value<string>());
                return Json["total_rows"].Value<int>();
            }
        }
        public int OffSet
        {
            get
            {
                if (Json["offset"] == null) throw new CouchException(request, response, Json["reason"].Value<string>());
                return Json["offset"].Value<int>();
            }
        }
        public IEnumerable<JToken> Rows
        {
            get
            {
                if (Json["rows"] == null) throw new CouchException(request, response, Json["reason"].Value<string>());
                return (JArray)Json["rows"];
            }
        }
        /// <summary>
        /// Only populated when IncludeDocs is true
        /// </summary>
        public IEnumerable<JToken> Docs
        {
            get
            {
                return (JArray)Json["doc"];
            }
        }

        public bool IncludeDocs { get; private set; }

        public JToken[] Keys
        {
            get
            {
                var arry = (JArray)Json["rows"];
                return arry.Select(item => item["key"]).ToArray();
            }
        }
        /// <summary>
        /// An IEnumerable of strings insteda of the IEnumerable of JTokens
        /// </summary>
        public IEnumerable<string> RawRows
        {
            get
            {
                var arry = (JArray)Json["rows"];
                return arry.Select(item => item.ToString());
            }
        }

        public IEnumerable<string> RawValues
        {
            get
            {
                var arry = (JArray)Json["rows"];
                return arry.Select(item => item["value"].ToString());
            }
        }
        public IEnumerable<string> RawDocs
        {
            get
            {
                var arry = (JArray)Json["rows"];
                return arry.Select(item => item["doc"].ToString());
            }
        }
        public string RawString
        {
            get { return response.ResponseString; }
        }

        public bool Equals(IListResult other)
        {
            if (string.IsNullOrEmpty(Etag) || string.IsNullOrEmpty(other.Etag)) return false;
            return Etag == other.Etag;
        }

        public override string ToString()
        {
            return response.ResponseString;
        }
        /// <summary>
        /// Provides a formatted version of the json returned from this Result.  (Avoid this method in favor of RawString as it's much more performant)
        /// </summary>
        public string FormattedResponse { get { return Json.ToString(Formatting.Indented); } }

        public HttpWebResponse Response
        {
            get { throw new NotImplementedException(); }
        }

    }
}