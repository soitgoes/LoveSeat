using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using System;

namespace LoveSeat.Core.Support
{
	public static class ResponseExtensionMethods
	{
        private static string _defaultJsonObject = "{}";

		public static Document GetCouchDocument(this HttpWebResponse response)
		{
		    var jobj = JObject.Parse(response.GetResponseString());
            return new Document(jobj);
		}

        public static string GetResponseString(this HttpWebResponse response)
        {
            try
            {
                using (var stream = response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        var result = streamReader.ReadToEnd();
                        if (string.IsNullOrEmpty(result))
                            return _defaultJsonObject;

                        return result;
                    }
                }
            }
            catch (Exception)
            {
                return _defaultJsonObject;
            }
        }

		public static CouchResponseObject GetJObject(this HttpWebResponse httpResponse)
		{
            var response = new CouchResponseObject(JObject.Parse(httpResponse.GetResponseString()));
		    response.StatusCode = (int)httpResponse.StatusCode;
		    return response;
		}
	}
}