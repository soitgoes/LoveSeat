using System;
using System.Net;

namespace LoveSeat.Support
{
    public interface ICouchConnection
    {
        Uri BaseUri { get; set; }
        string DatabaseName { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        AuthenticationType AuthenticationType { get; set; }
        DbType DbType { get; set; }

        Cookie SessionCookie { get; set; }
        int? Timeout { get; set; }

        void ConfigureBaseUri(string host, int port, bool isHttps);
    }

    public class CouchConnection : ICouchConnection
    {
        public CouchConnection()
        {
            
        }
        public Uri BaseUri { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
        public DbType DbType { get; set; }
        public Cookie SessionCookie { get; set; }
        public int? Timeout { get; set; }

        public void ConfigureBaseUri(string host, int port, bool isHttps)
        {
            BaseUri = GetBaseUri(host, port, isHttps);
        }

        private static Uri GetBaseUri(string host, int port, bool isHttps)
        {
            return new Uri(isHttps ? "https://" + host + ":" + port + "/" : "http://" + host + ":" + port + "/");
        }
    }
}