using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace LoveSeat.Support
{
	public static class CookieStore
	{
		private static TtlDictionary<string, Cookie> cookiestore = new TtlDictionary<string, Cookie>();

		public static TtlDictionary<string, Cookie> Cookiestore
		{
			get { return cookiestore; }
			set { cookiestore = value; }
		}

		public static void Clear()
		{
			cookiestore = new TtlDictionary<string, Cookie>();
		}
	}
}
