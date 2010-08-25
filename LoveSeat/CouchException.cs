using System;
using System.Net;

namespace LoveSeat
{
	public class CouchException : Exception
	{
		private readonly HttpStatusCode statusCode;

		public CouchException(string message, HttpStatusCode statusCode)
			: base(message)
		{
			this.statusCode = statusCode;
		}
		public CouchException(HttpStatusCode statusCode)
		{
			this.statusCode = statusCode;
		}
		public HttpStatusCode StatusCode
		{
			get
			{
				return statusCode;
			}
		}
	}
}