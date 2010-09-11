using System;
using System.Net;
using Newtonsoft.Json.Linq;

namespace LoveSeat.Support
{
	public class CouchException : Exception
	{
		private readonly JObject response;

		public CouchException(JObject response, HttpWebRequest request): base(response.ToString()+ request.ToString())
		{
			this.response = response;			
		}
		public string Error
		{
			get
			{
				return response["error"].Value<string>();
			}
		}
		public string Reason
		{
			get
			{
				return response["reason"].Value<string>();
			}
		}

	}
}