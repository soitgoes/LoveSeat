using System;
using LoveSeat.Cloudant;
using LoveSeat.Interfaces;
using Newtonsoft.Json;

namespace LoveSeat.Support
{
    /// <summary>
    /// Factory for creating Couch objects
    /// The factory is stateless and is safe to use a singleton or per operation in an injection framework
    /// </summary>
    public class CouchFactory : ICouchFactory
    {
        public IDocumentDatabase GetDatabase(ICouchConnection couchConnection)
        {
            IDocumentDatabase db = new CouchDatabase(couchConnection);

            if (couchConnection.DbType == DbType.Cloudant)
            {
                db = new CloudantDatabase(db, couchConnection, this);
            }

            return db;
        }

        public ICouchClient GetClient(ICouchConnection couchConnection)
        {
            return new CouchClient(couchConnection);
        }

        public ICouchRequest GetRequest(Uri uri, ICouchConnection couchConnection)
        {
            return GetRequest(uri, null, couchConnection);
        }

        public ICouchRequest GetRequest(Uri uri, string etag, ICouchConnection couchConnection)
        {
            CouchRequest request;
            if (AuthenticationType.Cookie == couchConnection.AuthenticationType)
            {
                request = new CouchRequest(uri, couchConnection.SessionCookie, etag);
            }
            else if (AuthenticationType.Basic == couchConnection.AuthenticationType) //Basic Authentication
            {
                request = new CouchRequest(uri, couchConnection.Username, couchConnection.Password);
            }
            else //default Cookie
            {
                request = new CouchRequest(uri, couchConnection.SessionCookie, etag);
            }
            if (couchConnection.Timeout.HasValue) request.Timeout(couchConnection.Timeout.Value);
            return request;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class SimpleResponse : IBaseResponse
    {
        public bool Ok { get; set; }
    }
}