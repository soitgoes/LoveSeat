using System.Net;
using Newtonsoft.Json.Linq;

namespace LoveSeat.Core.Support
{
    /// <summary>
    /// Repersent a web response from CouchDB database.
    /// </summary>
    public class CouchResponse
    {
        private readonly string _responseString;
        private readonly HttpStatusCode _statusCode;
        private readonly string _statusDescription;
        private readonly string _etag;

        public CouchResponse(HttpWebResponse response)
        {
            _responseString = response.GetResponseString();
            _statusCode = response.StatusCode;
            _statusDescription = response.StatusDescription;
            _etag = response.Headers["ETag"];
        }

        public string ResponseString
        {
            get { return _responseString; }
        }

        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
        }

        public string StatusDescription
        {
            get { return _statusDescription; }
        }

        public string ETag
        {
            get { return _etag; }
        }

        /// <summary>
        /// Get the response string as JSON object.
        /// </summary>
        /// <returns></returns>
        public CouchResponseObject GetJObject()
        {
            var resp = new CouchResponseObject(JObject.Parse(_responseString));
            resp.StatusCode = (int)_statusCode;
            return resp;
        }

        /// <summary>
        /// Get the response as raw document.
        /// </summary>
        /// <returns></returns>
        public Document GetCouchDocument()
        {
            var jobj = JObject.Parse(_responseString);
            return new Document(jobj);
        }
    }
}
