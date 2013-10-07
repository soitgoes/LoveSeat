using System;
using System.Net;
using System.Web;
using LoveSeat.Support;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
    // type of authentication used in request to CouchDB
    public enum AuthenticationType
    { Basic, Cookie };

    /// <summary>
    /// Used as the starting point for any communication with CouchDB
    /// </summary>
    public class CouchClient : CouchBase
    {
        // Authentication type used in request to CouchDB
        protected readonly AuthenticationType authType;

        /// <summary>
        /// This is only intended for use if your CouchDb is in Admin Party
        /// </summary>
        public CouchClient()
            : this("localhost", 5984, null, null, false, AuthenticationType.Basic)
        {
        }

        /// <summary>
        /// CouchClient constructor
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public CouchClient(string username, string password)
            : this("localhost", 5984, username, password, false, AuthenticationType.Basic)
        {
        }

        /// <summary>
        /// Constructs the CouchClient and gets an authentication cookie (10 min)
        /// </summary>
        /// <param name="host">The hostname of the CouchDB instance</param>
        /// <param name="port">The port of the CouchDB instance</param>
        /// <param name="username">The username of the CouchDB instance</param>Cou
        /// <param name="password">The password of the CouchDB instance</param>
        public CouchClient(string host, int port, string username, string password, bool isHttps, AuthenticationType aT)
            : base(username, password, aT)
        {
            if (isHttps == false)
            {
                baseUri = "http://" + host + ":" + port + "/";
            }
            else
            {
                baseUri = "https://" + host + ":" + port + "/";
            }

            authType = aT;

        }


        /// <summary>
        /// CouchClient constructor
        /// </summary>
        /// <param name="config"></param>
        public CouchClient(CouchConfiguration config)
            : this(config.Host, config.Port, config.User, config.Password, false, AuthenticationType.Basic)
        {
        }

        /// <summary>
        /// Triggers one way replication from the source to target.  If bidirection is needed call this method twice with the source and target args reversed.
        /// </summary>
        /// <param name="source">Uri or database name of database to replicate from</param>
        /// <param name="target">Uri or database name of database to replicate to</param>
        /// <param name="continuous">Whether or not CouchDB should continue to replicate going forward on it's own</param>
        /// <returns></returns>
        public CouchResponseObject TriggerReplication(string source, string target, bool continuous)
        {
            var request = GetRequest(baseUri + "_replicate");

            var options = new ReplicationOptions(source, target, continuous);
            var response = request.Post()
                .Data(options.ToString())
                .GetCouchResponse();

            return response.GetJObject();
        }

        public CouchResponseObject TriggerReplication(string source, string target)
        {
            return TriggerReplication(source, target, false);
        }

        /// <summary>
        /// Creates a database
        /// </summary>
        /// <param name="databaseName">Name of new database</param>
        /// <returns></returns>
        public CouchResponseObject CreateDatabase(string databaseName)
        {
            return GetRequest(baseUri + databaseName).Put().GetCouchResponse().GetJObject();
        }
        /// <summary>
        /// Deletes the specified database
        /// </summary>
        /// <param name="databaseName">Database to delete</param>
        /// <returns></returns>
        public CouchResponseObject DeleteDatabase(string databaseName)
        {
            return GetRequest(baseUri + databaseName).Delete().GetCouchResponse().GetJObject();
        }

        /// <summary>
        /// Gets a Database object
        /// </summary>
        /// <param name="databaseName">Name of database to fetch</param>
        /// <returns></returns>
        public CouchDatabase GetDatabase(string databaseName)
        {
            return new CouchDatabase(baseUri, databaseName, username, password, this.authType);
        }

        /// <summary>
        /// Creates an admin user
        /// </summary>
        /// <param name="usernameToCreate"></param>
        /// <param name="passwordToCreate"></param>
        /// <returns></returns>
        public CouchResponseObject CreateAdminUser(string usernameToCreate, string passwordToCreate)
        {
            //Creates the user in the local.ini
            var iniResult = GetRequest(baseUri + "_config/admins/" + HttpUtility.UrlEncode(usernameToCreate))
                .Put().Json().Data("\"" + passwordToCreate + "\"").GetCouchResponse();

            var user = @"{ ""name"": ""%name%"",
  ""_id"": ""org.couchdb.user:%name%"", ""type"": ""user"", ""roles"": [],
}".Replace("%name%", usernameToCreate).Replace("\r\n", "");
            var docResult = GetRequest(baseUri + "_users/org.couchdb.user:" + HttpUtility.UrlEncode(usernameToCreate))
                .Put().Json().Data(user).GetCouchResponse().GetJObject();
            return docResult;

        }

        /// <summary>
        /// Deletes admin user  (if you have permission)
        /// </summary>
        /// <param name="userToDelete"></param>
        public void DeleteAdminUser(string userToDelete)
        {
            var iniResult = GetRequest(baseUri + "_config/admins/" + HttpUtility.UrlEncode(userToDelete))
                .Delete().Json().GetCouchResponse();

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
      public bool HasDatabase(string databaseName) {
            var request = GetRequest(baseUri + databaseName).Timeout(-1);

            var response = request.GetCouchResponse();
            var pDocResult = new Document(response.ResponseString);

            if (pDocResult["error"] == null) {
                return (true);
            }
            if (pDocResult["error"].Value<String>() == "not_found") {
                return (false);
            }
            throw new Exception(pDocResult["error"].Value<String>());
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
        public Document GetUser(string userId)
        {
            var db = new CouchDatabase(baseUri, "_users", username, password, this.authType);
            userId = "org.couchdb.user:" + HttpUtility.UrlEncode(userId);
            return db.GetDocument(userId);
        }


        /// <summary>
        /// Create a user in the _user database
        /// </summary>
        /// <param name="usernameToCreate"></param>
        /// <param name="passwordToCreate"></param>
        /// <returns></returns>
        public CouchResponseObject CreateUser(string usernameToCreate, string passwordToCreate)
        {

            var user = "";
            //check if user exists already
            Document cUser = GetUser(usernameToCreate);

            if (cUser != null) // add revision number to update existing document
            {
                user = @"{ ""name"": ""%name%"",""_id"": ""org.couchdb.user:%name%"",""_rev"": ""%_rev%"", ""type"": ""user"", ""roles"": [],""password_sha"": ""%password%"",""salt"": ""%salt%""}"
                   .Replace("%_rev%", cUser.Rev);
            }
            else // new user to add
            {
                user = @"{ ""name"": ""%name%"",""_id"": ""org.couchdb.user:%name%"", ""type"": ""user"", ""roles"": [],""password_sha"": ""%password%"",""salt"": ""%salt%""}";
            }

            //Add/Update user to _user
            string salt = new string(new char[0]); // Empty string

            string hashedPassword = HashIt.ComputeHash(passwordToCreate, ref salt);

            user = user.Replace("%name%", usernameToCreate)
                .Replace("%password%", hashedPassword)
                .Replace("%salt%", salt)
                .Replace("\r\n", "");

            var docResult = GetRequest(baseUri + "_users/org.couchdb.user:" + HttpUtility.UrlEncode(usernameToCreate))
                .Put().Json().Data(user).GetCouchResponse();

            if (docResult.StatusCode == HttpStatusCode.Created)
            {
                return docResult.GetJObject();
            }
            else
            {
                throw new WebException("An error occurred when creating a user.  Status code: " + docResult.StatusDescription);
            }
        }

        /// <summary>
        /// Deletes user  (if you have permission)
        /// </summary>
        /// <param name="userToDelete"></param>
        public void DeleteUser(string userToDelete)
        {
            var userDb = this.GetDatabase("_users");
            var userId = "org.couchdb.user:" + HttpUtility.UrlEncode(userToDelete);
            var userDoc = userDb.GetDocument(userId);
            if (userDoc != null)
            {
                userDb.DeleteDocument(userDoc.Id, userDoc.Rev);
            }
            else
            {
                throw new WebException("An error occurred while deleting the user " + userToDelete + ". Username does not exist.");
            }

        }

        /// <summary>
        /// Get's UUID from CouchDB.  Limit 50 uuid requests.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public UniqueIdentifiers GetUUID(int count)
        {

            string request = baseUri + "_uuids";

            if (count == 1)
            {
                if (count > 50)//limit 50
                {
                    count = 50;
                }

                request = request + "?Count=" + Convert.ToString(count);
            }

            var x = GetRequest(request);
            string str = x.Get().Json().GetCouchResponse().GetJObject().ToString();
            UniqueIdentifiers y = Newtonsoft.Json.JsonConvert.DeserializeObject<UniqueIdentifiers>(str);

            return y;

        }
    }

    /// <summary>
    /// Unique Identifier
    /// </summary>
    public class UniqueIdentifiers
    {
        public System.Collections.Generic.List<String> uuids { get; set; }
    }
}



