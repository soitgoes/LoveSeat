using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace LoveSeat.Support
{
	public static class ResponseExtensionMethods
	{
		public static CouchDocument GetCouchDocument(this HttpWebResponse response)
		{
			using (var stream = response.GetResponseStream())
			{
				using (var streamReader = new StreamReader(stream))
				{
					var result = streamReader.ReadToEnd();
					return new CouchDocument(JObject.Parse(result)); 
				}
			}
		}
		public static JObject GetJObject(this HttpWebResponse response)
		{
			using (var stream = response.GetResponseStream())
			{
				using (var streamReader = new StreamReader(stream))
				{
					var result = streamReader.ReadToEnd();
					return JObject.Parse(result);
				}
			}
		}
	}
}