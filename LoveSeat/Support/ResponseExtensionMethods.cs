using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace LoveSeat.Support
{
	public static class ResponseExtensionMethods
	{
		public static Document GetCouchDocument(this HttpWebResponse response)
		{
		    var jobj = JObject.Parse(response.GetResponseString());
            return new Document(jobj);
		}

        public static string GetResponseString(this HttpWebResponse response)
        {
            using (var stream = response.GetResponseStream())
			{
			    if (stream != null)
			    {
			        using (var streamReader = new StreamReader(stream))
			        {
			            var result = streamReader.ReadToEnd();
			            return string.IsNullOrEmpty(result) ? null : result;
			        }
			    }
			}

            return null;
        }

		public static CouchResponse GetJObject(this HttpWebResponse response)
		{
            return new CouchResponse(JObject.Parse(response.GetResponseString()))
            {
                StatusCode = (int) response.StatusCode
            };
		}
	}
}