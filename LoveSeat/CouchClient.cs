using System;
using System.Web;
using LoveSeat.Support;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
	/// <summary>
	/// Used as the starting point for any communication with CouchDB
	/// </summary>
	public class CouchClient : CouchBase
	{
		/// <summary>
		/// This is only intended for use if your CouchDb is in Admin Party
		/// </summary>
		public CouchClient()
			: this("localhost", 5984, null, null)
		{
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		public CouchClient(string username, string password)
			: this("localhost", 5984, username, password)
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

			return response.GetCouchDocument();
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
			return GetRequest(baseUri + databaseName).Put().GetResponse().GetCouchDocument();
		}
		/// <summary>
		/// Deletes the specified database
		/// </summary>
		/// <param name="databaseName">Database to delete</param>
		/// <returns></returns>
		public JObject DeleteDatabase(string databaseName)
		{
			return GetRequest(baseUri + databaseName).Delete().GetResponse().GetCouchDocument();
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

		public JObject CreateAdminUser(string usernameToCreate, string passwordToCreate)
		{
			try
			{
				//Creates the user in the local.ini
				var iniResult = GetRequest(baseUri + "_config/admins/" + HttpUtility.UrlEncode(usernameToCreate))
					.Put().Json().Data("\"" + passwordToCreate + "\"").GetResponse();
			}
			catch (CouchException ce)
			{
				//fail silently
			}
			var user = @"{ ""name"": ""%name%"",
  ""_id"": ""org.couchdb.user:%name%"", ""type"": ""user"", ""roles"": [],
}".Replace("%name%", usernameToCreate).Replace("\r\n", "");
			var docResult = GetRequest(baseUri + "_users/org.couchdb.user:" + HttpUtility.UrlEncode(usernameToCreate))
				.Put().Json().Data(user).GetResponse().GetCouchDocument();
			return docResult;

		}
		/// <summary>
		/// Deletes user  (if you have permission)
		/// </summary>
		/// <param name="userToDelete"></param>
		public void DeleteAdminUser(string userToDelete)
		{
			try
			{
				var iniResult = GetRequest(baseUri + "_config/admins/" + HttpUtility.UrlEncode(userToDelete))
					.Delete().Json().GetResponse();
			}
			catch (CouchException ce)
			{
				//fail silently
			}

			var userDb = this.GetDatabase("_users");
			var userId = "org.couchdb.user:" + HttpUtility.UrlEncode(userToDelete);
			var userDoc = userDb.GetDocument(userId);
			if (userDoc != null)
			{
				userDb.DeleteDocument(userDoc.Id, userDoc.Rev);	
			}
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
					.GetCouchDocument()["ok"];
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
		/// <summary>
		/// Returns true/false depending on whether or not the user is contained in the _users database
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public bool HasUser(string userId)
		{
			return GetUser(userId) != null;
		}
		/// <summary>
		/// Get's the user.  
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public CouchDocument GetUser(string userId)
		{
			var db = new CouchDatabase(baseUri, "_users", username, password);
			userId = "org.couchdb.user:" + HttpUtility.UrlEncode(userId);
			return db.GetDocument(userId);
		}
	}
}
