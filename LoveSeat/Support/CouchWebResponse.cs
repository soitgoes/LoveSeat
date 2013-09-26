using System.Net;
using Newtonsoft.Json.Linq;

namespace LoveSeat.Support
{
    /// <summary>
    /// Repersent a web response from CouchDB database.
    /// </summary>
    public class CouchWebResponse
    {
        private readonly string responseString;
        private readonly HttpStatusCode statusCode;
        private readonly string statusDescription;
        private readonly string etag;

        public CouchWebResponse(HttpWebResponse response)
        {
            responseString = response.GetResponseString();
            statusCode = response.StatusCode;
            statusDescription = response.StatusDescription;
            etag = response.Headers["ETag"];
        }

        public string ResponseString { get { return responseString; } }

        public HttpStatusCode StatusCode { get { return statusCode; } }

        public string StatusDescription { get { return statusDescription; } }

        public string ETag { get { return etag; } }

        public CouchResponse GetJObject()
        {
            var resp = new CouchResponse(JObject.Parse(responseString));
            resp.StatusCode = (int)statusCode;
            return resp;
        }

        public Document GetCouchDocument()
        {
            var jobj = JObject.Parse(responseString);
            return new Document(jobj);
        }

    }
}
