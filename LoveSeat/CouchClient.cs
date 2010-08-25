using System;
using LoveSeat.Support;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
	/// <summary>
	/// Used as the starting point for any communication with CouchDB
	/// </summary>
	public class CouchClient : CouchBase
	{
		private CouchClient()
		{
			//hides ctor
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		public CouchClient(string username, string password) : this("localhost", 5984, username, password)
		{
		}

		/// <summary>
		/// Constructs the CouchClient and gets an authentication cookie (10 min)
		/// </summary>
		/// <param name="host">The hostname of the CouchDB instance</param>
		/// <param name="port">The port of the CouchDB instance</param>
		/// <param name="username">The username of the CouchDB instance</param>
		/// <param name="password">The password of the CouchDB instance</param>
		public CouchClient(string host, int port, string username, string password)
			: base(username, password)
		{
			baseUri = "http://" + host + ":" + port + "/";
		}

		/// <summary>
		/// Triggers one way replication from the source to target.  If bidirection is needed call this method twice with the source and target args reversed.
		/// </summary>
		/// <param name="source">Uri or database name of database to replicate from</param>
		/// <param name="target">Uri or database name of database to replicate to</param>
		/// <param name="continuous">Whether or not CouchDB should continue to replicate going forward on it's own</param>
		/// <returns></returns>
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

		/// <summary>
		/// Creates a database
		/// </summary>
		/// <param name="databaseName">Name of new database</param>
		/// <returns></returns>
		public JObject CreateDatabase(string databaseName)
		{
			return GetRequest(baseUri + databaseName).Put().GetResponse().GetJObject();
		}
		/// <summary>
		/// Deletes the specified database
		/// </summary>
		/// <param name="databaseName">Database to delete</param>
		/// <returns></returns>
		public JObject DeleteDatabase(string databaseName)
		{
			return GetRequest(baseUri + databaseName).Delete().GetResponse().GetJObject();
		}

		/// <summary>
		/// Gets a Database object
		/// </summary>
		/// <param name="databaseName">Name of database to fetch</param>
		/// <returns></returns>
		public CouchDatabase GetDatabase(string databaseName)
		{
			return new CouchDatabase(baseUri, databaseName, username, password);
		}

		/// <summary>
		/// Returns a bool indicating whether or not the database exists.
		/// </summary>
		/// <param name="databaseName"></param>
		/// <returns></returns>
		public bool HasDatabase(string databaseName)
		{
			try
			{
				var result = GetRequest(baseUri + databaseName)
					.Get()
					.GetResponse()
					.GetJObject()["ok"];
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

	}
}
