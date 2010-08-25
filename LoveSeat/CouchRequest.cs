using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
	internal class CouchRequest
	{
		private readonly HttpWebRequest request;

		public CouchRequest(string uri)
		{
			this.request = (HttpWebRequest)WebRequest.Create(uri);
			this.request.ContentType = "application/json";
		}

		public CouchRequest Put()
		{
			request.Method = "PUT";
			return this;
		}

		public CouchRequest Get()
		{
			request.Method = "GET";
			return this;
		}
		public CouchRequest Post()
		{
			request.Method = "POST";
			return this;
		}
		public CouchRequest Delete()
		{
			request.Method = "Delete";
			return this;
		}
		public CouchRequest Data(string data)
		{
			using (var body = request.GetRequestStream())
			{
				var encodedData = Encoding.UTF8.GetBytes(data);
				body.Write(encodedData, 0, encodedData.Length);
			}
			return this;
		}

		public CouchRequest Data(JObject obj)
		{
			return this.Data(obj.ToString());
		}

		public CouchRequest ContentType(string contentType)
		{
			request.ContentType = contentType;
			return this;
		}

		public HttpWebResponse GetResponse()
		{
			try
			{
				var response = (HttpWebResponse)request.GetResponse();
				return response;
			}
			catch (WebException webEx)
			{
				var response = (HttpWebResponse)webEx.Response;
				using (var stream = response.GetResponseStream())
				{
					using (var streamReader = new StreamReader(stream))
					{
						var errorMessage = streamReader.ReadToEnd();
						if (string.IsNullOrEmpty(errorMessage))
						{
							throw new CouchException(response.StatusCode);
						}
						throw new CouchException(errorMessage, response.StatusCode);
					}
				}
			}
		}
	}
}