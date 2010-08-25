using System;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
	/// <summary>
	/// Used as the starting point for any communication with CouchDB
	/// </summary>
	public class CouchClient : CouchBase
	{
		/// <summary>
		/// Constructs the CouchClient and gets an authentication cookie (10 min)
		/// </summary>
		/// <param name="host">The hostname of the CouchDB instance</param>
		/// <param name="port">The port of the CouchDB instance</param>
		/// <param name="username">The username of the CouchDB instance</param>
		/// <param name="password">The password of the CouchDB instance</param>
		public CouchClient(string host, int port, string username, string password):base(username, password)
		{
			baseUri = "http://" + host + ":" + port + "/";
		}

		public JObject TriggerReplication(string source, string target, bool continuous)
		{
			var request = GetRequest(baseUri + "_replicate");
			
			var options = new ReplicationOptions(source, target, continuous);
			var response = request.Post()
				.Data(options.ToString())
				.GetResponse();

			return response.GetJObject();
		}

		

		public JObject TriggerReplication(string source, string target)
		{
			return TriggerReplication(source, target, false);
		}

		public JObject CreateDatabase(string databaseName)
		{
			return GetRequest(baseUri + databaseName).Put().GetResponse().GetJObject();
		}

		public JObject DeleteDatabase(string databaseName)
		{
			return GetRequest(baseUri + databaseName).Delete().GetResponse().GetJObject();
		}
		public CouchDatabase GetDatabase(string databaseName)
		{
			return new CouchDatabase(baseUri , databaseName, username, password);
		}

		public bool HasDatabase(string databaseName)
		{
			try
			{
				var result = GetRequest(baseUri + databaseName)
					.Get()
					.GetResponse()
					.GetJObject()["ok"];
				return true;
			}catch(Exception ex)
			{
				return false;
			}
		}
		
	}
}
