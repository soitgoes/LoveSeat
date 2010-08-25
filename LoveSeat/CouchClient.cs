using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
	public class CouchClient
	{
		private readonly string host;
		private readonly int port;
		private readonly string username;
		private readonly string password;
		private readonly string baseUri;

		/// <summary>
		/// Constructs the CouchClient and gets an authentication cookie (10 min)
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		/// <param name="username"></param>
		/// <param name="password"></param>
		public CouchClient(string host, int port, string username, string password)
		{
			this.host = host;
			this.port = port;
			this.username = username;
			this.password = password;
			this.baseUri = "http://" + host + ":" + port + "/";
		}
		public Cookie GetSession()
		{
			var request = new CouchRequest(baseUri + "_session");
			var response = request.Post()
				.ContentType("application/x-www-form-urlencoded")
				.Data("name=" + username + "&password=" + password)
				.GetResponse();

			var header = response.Headers.Get("Set-Cookie");
			if (header != null)
			{
				var parts = header.Split(';')[0].Split('=');
				var authCookie = new Cookie(parts[0], parts[1]);
				authCookie.Domain = response.Server;
				return authCookie;
			}
			return null;
		}
		public JObject TriggerReplication(string source, string target, bool continuous)
		{
			var request = GetRequest(baseUri + "_replicate");
			
			var options = new ReplicationOptions(source, target, continuous);
			var response = request.Post()
				.Data(options.ToString())
				.GetResponse();

			using ( var stream = response.GetResponseStream())
			{
				using (var streamReader = new StreamReader(stream))
				{
					var result = streamReader.ReadToEnd();
					return JObject.Parse(result);
				}
			}
		}
		public JObject TriggerReplication(string source, string target)
		{
			return TriggerReplication(source, target, false);
		}
		internal CouchRequest GetRequest(string uri)
		{
			var request = new CouchRequest(uri, GetSession());			
			return request;
		}
	}
}
