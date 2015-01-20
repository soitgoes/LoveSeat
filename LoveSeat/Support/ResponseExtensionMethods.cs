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
				using (var streamReader = new StreamReader(stream))
				{
					var result = streamReader.ReadToEnd();
                    if (string.IsNullOrEmpty(result)) return null;
					return result;
				}
			}
        }
		public static CouchResponseObject GetJObject(this HttpWebResponse response)
		{
            var resp = new CouchResponseObject(JObject.Parse(response.GetResponseString()));
		    resp.StatusCode = (int)response.StatusCode;
		    return resp;
		}
	}
}