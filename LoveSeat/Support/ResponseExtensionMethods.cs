using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace LoveSeat.Support
{
	public static class ResponseExtensionMethods
	{
		public static CouchDocument GetCouchDocument(this HttpWebResponse response)
		{
			return new CouchDocument(response.GetJObject()); 
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
		public static JObject GetJObject(this HttpWebResponse response)
		{
		    return JObject.Parse(response.GetResponseString());
		}
	}
}