using System.Net;
using Newtonsoft.Json.Linq;

namespace LoveSeat.Support
{
    public interface ICouchResponse
    {
        string ResponseString { get; }
        HttpStatusCode StatusCode { get; }
        string StatusDescription { get; }
        string ETag { get; }

        /// <summary>
        /// Get the response string as JSON object.
        /// </summary>
        /// <returns></returns>
        CouchResponseObject GetJObject();

        /// <summary>
        /// Get the response as raw document.
        /// </summary>
        /// <returns></returns>
        Document GetCouchDocument();
    }

    /// <summary>
    /// Repersent a web response from CouchDB database.
    /// </summary>
    public class CouchResponse : ICouchResponse
    {
        private readonly string responseString;
        private readonly HttpStatusCode statusCode;
        private readonly string statusDescription;
        private readonly string etag;

        public CouchResponse(HttpWebResponse response)
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

        /// <summary>
        /// Get the response string as JSON object.
        /// </summary>
        /// <returns></returns>
        public CouchResponseObject GetJObject()
        {
            var resp = new CouchResponseObject(JObject.Parse(responseString));
            resp.StatusCode = (int)statusCode;
            return resp;
        }

        /// <summary>
        /// Get the response as raw document.
        /// </summary>
        /// <returns></returns>
        public Document GetCouchDocument()
        {
            var jobj = JObject.Parse(responseString);
            return new Document(jobj);
        }

    }
}
