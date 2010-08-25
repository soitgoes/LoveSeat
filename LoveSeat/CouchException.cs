using System;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
	public class CouchException : Exception
	{
		private readonly JObject response;

		public CouchException(JObject response): base(response.ToString())
		{
			this.response = response;			
		}
		public string Error
		{
			get
			{
				return response["error"].ToString();
			}
		}
		public string Reason
		{
			get
			{
				return response["reason"].ToString();
			}
		}

	}
}