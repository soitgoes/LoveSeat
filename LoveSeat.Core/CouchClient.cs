using System;
using System.Net;
using Newtonsoft.Json.Linq;
using LoveSeat.Core.Support;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace LoveSeat.Core
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
        public CouchClient(string host, int port, string username, string password, bool isHttps, AuthenticationType authType)
            : base(username, password, authType)
        {
            if (isHttps == false)
                _baseUri = "http://" + host + ":" + port + "/";
            else
                _baseUri = "https://" + host + ":" + port + "/";

            _authType = authType;
        }

        /// <summary>
        /// The full uri for the couch constructor
        /// </summary>
        /// <param name="baseUri">Base Url with authentication to couch.  Should NOT include database</param>
        public CouchClient(string baseUri)
        {
            _baseUri = baseUri.EndsWith("/") ? baseUri : baseUri + "/"; //Ensure trailing slash
            _authType = AuthenticationType.Basic;
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
        public CouchResponseObject TriggerReplication(string source, string target, bool continuous = false)
        {
            return TriggerReplicationAsync(source, target, continuous).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a database
        /// </summary>
        /// <param name="databaseName">Name of new database</param>
        /// <returns></returns>
        public CouchResponseObject CreateDatabase(string databaseName)
        {
            return CreateDatabaseAsync(databaseName).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Deletes the specified database
        /// </summary>
        /// <param name="databaseName">Database to delete</param>
        /// <returns></returns>
        public CouchResponseObject DeleteDatabase(string databaseName)
        {
            return DeleteDatabaseAsync(databaseName).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a Database object
        /// </summary>
        /// <param name="databaseName">Name of database to fetch</param>
        /// <returns></returns>
        public CouchDatabase GetDatabase(string databaseName)
        {
            return new CouchDatabase(_baseUri, databaseName, _username, _password, _authType);
        }

        /// <summary>
        /// Creates an admin user
        /// </summary>
        /// <param name="usernameToCreate"></param>
        /// <param name="passwordToCreate"></param>
        /// <returns></returns>
        public CouchResponseObject CreateAdminUser(string usernameToCreate, string passwordToCreate)
        {
            return CreateAdminUserAsync(usernameToCreate, passwordToCreate).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Deletes admin user  (if you have permission)
        /// </summary>
        /// <param name="userToDelete"></param>
        public void DeleteAdminUser(string userToDelete)
        {
            DeleteAdminUserAsync(userToDelete).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Returns a bool indicating whether or not the database exists.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public bool HasDatabase(string databaseName)
        {
            return HasDatabaseAsync(databaseName).GetAwaiter().GetResult();
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
            var db = new CouchDatabase(_baseUri, "_users", _username, _password, this._authType);
            userId = "org.couchdb.user:" + WebUtility.UrlEncode(userId);
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
            return CreateUserAsync(usernameToCreate, passwordToCreate).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Deletes user  (if you have permission)
        /// </summary>
        /// <param name="userToDelete"></param>
        public void DeleteUser(string userToDelete)
        {
            var userDb = GetDatabase("_users");
            var userId = "org.couchdb.user:" + WebUtility.UrlEncode(userToDelete);
            var userDoc = userDb.GetDocument(userId);

            if (userDoc != null)
                userDb.DeleteDocument(userDoc.Id, userDoc.Rev);
            else
                throw new Exception("An error occurred while deleting the user " + userToDelete + ". Username does not exist.");
        }

        /// <summary>
        /// Get's UUID from CouchDB.  Limit 50 uuid requests.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public UniqueIdentifiers GetUUID(int count)
        {
            return GetUUIDAsync(count).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a database
        /// </summary>
        /// <param name="databaseName">Name of new database</param>
        /// <returns></returns>
        public async Task<CouchResponseObject> CreateDatabaseAsync(string databaseName)
        {
            return
                (await
                (await GetRequestAsync(_baseUri + databaseName).ConfigureAwait(false))
                .Put()
                .GetCouchResponseAsync().ConfigureAwait(false))
                .GetJObject();
        }

        /// <summary>
        /// Deletes the specified database
        /// </summary>
        /// <param name="databaseName">Database to delete</param>
        /// <returns></returns>
        public async Task<CouchResponseObject> DeleteDatabaseAsync(string databaseName)
        {
            return
                (await
                (await GetRequestAsync(_baseUri + databaseName).ConfigureAwait(false))
                .Delete()
                .GetCouchResponseAsync().ConfigureAwait(false))
                .GetJObject();
        }

        /// <summary>
        /// Triggers one way replication from the source to target.  If bidirection is needed call this method twice with the source and target args reversed.
        /// </summary>
        /// <param name="source">Uri or database name of database to replicate from</param>
        /// <param name="target">Uri or database name of database to replicate to</param>
        /// <param name="continuous">Whether or not CouchDB should continue to replicate going forward on it's own</param>
        /// <returns></returns>
        public async Task<CouchResponseObject> TriggerReplicationAsync(string source, string target, bool continuous = false)
        {
            var request = await GetRequestAsync(_baseUri + "_replicate").ConfigureAwait(false);

            var options = new ReplicationOptions(source, target, continuous);

            var response =
                await (await request
                .Post()
                .DataAsync(options.ToString()).ConfigureAwait(false))
                .GetCouchResponseAsync().ConfigureAwait(false);

            return response.GetJObject();
        }

        /// <summary>
        /// Creates an admin user
        /// </summary>
        /// <param name="usernameToCreate"></param>
        /// <param name="passwordToCreate"></param>
        /// <returns></returns>
        public async Task<CouchResponseObject> CreateAdminUserAsync(string usernameToCreate, string passwordToCreate)
        {
            // Creates the user in the local.ini
            var response = await
                (await
                (await GetRequestAsync(_baseUri + "_config/admins/" + WebUtility.UrlEncode(usernameToCreate)).ConfigureAwait(false))
                .Put()
                .Json()
                .DataAsync("\"" + passwordToCreate + "\"")
                .ConfigureAwait(false))
                .GetCouchResponseAsync()
                .ConfigureAwait(false);

            var user = @"{ ""name"": ""%name%"",
                       ""_id"": ""org.couchdb.user:%name%"", ""type"": ""user"", ""roles"": [],
                       }".Replace("%name%", usernameToCreate).Replace("\r\n", "");

            var docResult =
                (await
                (await
                (await GetRequestAsync(_baseUri + "_users/org.couchdb.user:" + WebUtility.UrlEncode(usernameToCreate)).ConfigureAwait(false))
                .Put()
                .Json()
                .DataAsync(user).ConfigureAwait(false))
                .GetCouchResponseAsync().ConfigureAwait(false))
                .GetJObject();

            return docResult;

        }

        public async Task DeleteAdminUserAsync(string userToDelete)
        {
            var iniResult = await
                (await GetRequestAsync(_baseUri + "_config/admins/" + WebUtility.UrlEncode(userToDelete)).ConfigureAwait(false))
                .Delete()
                .Json()
                .GetCouchResponseAsync().ConfigureAwait(false);

            var userDb = GetDatabase("_users");
            var userId = "org.couchdb.user:" + WebUtility.UrlEncode(userToDelete);
            var userDoc = userDb.GetDocument(userId);

            if (userDoc != null)
                userDb.DeleteDocument(userDoc.Id, userDoc.Rev);
        }

        public async Task<bool> HasDatabaseAsync(string databaseName)
        {
            var request = (await GetRequestAsync(_baseUri + databaseName)
                .ConfigureAwait(false))
                .Timeout(-1);

            var response = await request.GetCouchResponseAsync().ConfigureAwait(false);
            var pDocResult = new Document(response.ResponseString);

            if (pDocResult.JObject["error"] == null)
                return true;

            if (pDocResult.JObject["error"].Value<String>() == "not_found")
                return false;

            throw new Exception(pDocResult.JObject["error"].Value<String>());
        }

        public async Task<CouchResponseObject> CreateUserAsync(string usernameToCreate, string passwordToCreate)
        {
            var user = string.Empty;

            // check if user exists already
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

            // Add/Update user to _user
            string salt = new string(new char[0]); // Empty string

            string hashedPassword = HashIt.ComputeHash(passwordToCreate, ref salt);

            user = user.Replace("%name%", usernameToCreate)
                .Replace("%password%", hashedPassword)
                .Replace("%salt%", salt)
                .Replace("\r\n", "");

            var docResult = await (await
                (await GetRequestAsync(_baseUri + "_users/org.couchdb.user:" + WebUtility.UrlEncode(usernameToCreate))
                .ConfigureAwait(false))
                .Put()
                .Json()
                .DataAsync(user).ConfigureAwait(false))
                .GetCouchResponseAsync().ConfigureAwait(false);

            if (docResult.StatusCode == HttpStatusCode.Created)
                return docResult.GetJObject();
            else
                throw new Exception("An error occurred when creating a user.  Status code: " + docResult.StatusDescription);
        }

        public async Task<UniqueIdentifiers> GetUUIDAsync(int count)
        {
            string requestUri = _baseUri + "_uuids";

            if (count == 1)
            {
                if (count > 50) // limit 50
                    count = 50;

                requestUri = requestUri + "?Count=" + Convert.ToString(count);
            }

            var request = await GetRequestAsync(requestUri).ConfigureAwait(false);

            string response = (await request
                .Get()
                .Json()
                .GetCouchResponseAsync().ConfigureAwait(false))
                .GetJObject()
                .ToString();

            UniqueIdentifiers uniqueIdentifiers = JsonConvert.DeserializeObject<UniqueIdentifiers>(response);
            return uniqueIdentifiers;
        }
    }
}



