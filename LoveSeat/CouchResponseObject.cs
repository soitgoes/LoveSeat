using Newtonsoft.Json.Linq;

namespace LoveSeat
{
    /// <summary>
    /// Repersent response object from CouchDB as JSON.
    /// </summary>
    public class CouchResponseObject : JObject
    {
        public CouchResponseObject(JObject obj)
            : base(obj)
        {
        }

        /// <summary>
        /// Get the HTTP status code of the response.
        /// </summary>
        public int StatusCode { get; set; }
    }
}
