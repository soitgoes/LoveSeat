using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LoveSeat.Core.Interfaces;
using LoveSeat.Core.Support;

namespace LoveSeat.Core
{
    public class ViewResult : IViewResult
    {
        private JObject _json = null;
        private readonly CouchResponse _response;
        private readonly HttpWebRequest _request;

        public ViewResult(CouchResponse response, HttpWebRequest request, bool includeDocs = false)
        {
            _response = response;
            _request = request;
            IncludeDocs = includeDocs;
        }

        public JObject Json
        {
            get
            {
                return _json ?? (_json = JObject.Parse(_response.ResponseString));
            }
        }

        /// <summary>
        /// Typically won't be needed.  Provided for debuging assistance
        /// </summary>
        public HttpWebRequest Request
        {
            get { return _request; }
        }

        public HttpStatusCode StatusCode
        {
            get { return _response.StatusCode; }
        }

        public string Etag
        {
            get { return _response.ETag; }
        }

        public int TotalRows
        {
            get
            {
                if (Json["total_rows"] == null)
                    throw new CouchException(_request, _response, Json["reason"].Value<string>());

                return Json["total_rows"].Value<int>();
            }
        }

        public int OffSet
        {
            get
            {
                if (Json["offset"] == null)
                    throw new CouchException(_request, _response, Json["reason"].Value<string>());

                return Json["offset"].Value<int>();
            }
        }

        public IEnumerable<JToken> Rows
        {
            get
            {
                if (Json["rows"] == null)
                    throw new CouchException(_request, _response, Json["reason"].Value<string>());

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
                return Rows.Select(x => x["doc"]);
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

                return arry == null
                    ? new List<string>()
                    : arry.Select(item => item.ToString());
            }
        }

        public IEnumerable<string> RawValues
        {
            get
            {
                var arry = (JArray)Json["rows"];

                return arry == null
                    ? new List<string>()
                    : arry.Select(item => item["value"].ToString());
            }
        }

        public IEnumerable<string> RawDocs
        {
            get
            {
                var arry = (JArray)Json["rows"];

                return arry == null
                    ? new List<string>()
                    : arry.Select(item => item["doc"].ToString());
            }
        }

        public string RawString
        {
            get { return _response.ResponseString; }
        }

        /// <summary>
        /// Provides a formatted version of the json returned from this Result.  (Avoid this method in favor of RawString as it's much more performant)
        /// </summary>
        public string FormattedResponse
        {
            get { return Json.ToString(Formatting.Indented); }
        }

        public HttpWebResponse Response
        {
            get { throw new NotImplementedException(); }
        }

        public bool Equals(IListResult other)
        {
            if (string.IsNullOrEmpty(Etag) || string.IsNullOrEmpty(other.Etag))
                return false;

            return Etag == other.Etag;
        }

        public override string ToString()
        {
            return _response.ResponseString;
        }
    }
}