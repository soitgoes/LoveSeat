using System;
using System.Net;

namespace LoveSeat.Support
{
    public abstract class CouchBase
    {
        protected readonly string username;
        protected readonly string password;
        protected readonly AuthenticationType authType;
        protected string baseUri;
        private TtlDictionary<string, Cookie> cookiestore = new TtlDictionary<string, Cookie>();
        private int? timeout;

        protected CouchBase()
        {
            throw new Exception("Should not be used.");
        }
        protected CouchBase(string username, string password, AuthenticationType aT)
        {
            this.username = username;
            this.password = password;
            this.authType = aT;
        }
        public static bool Authenticate(string baseUri, string userName, string password)
        {
            if (!baseUri.Contains("http://"))
                baseUri = "http://" + baseUri;
            var request = new CouchRequest(baseUri + "/_session");
            var response = request.Post()
                .ContentType("application/x-www-form-urlencoded")
                .Data("name=" + userName + "&password=" + password)
                .Timeout(3000)
                .GetCouchResponse();
            return response.StatusCode == HttpStatusCode.OK;
        }
        public Cookie GetSession() {
            var authCookie = cookiestore["authcookie"];

            if (authCookie != null)
                return authCookie;

            if (string.IsNullOrEmpty(username)) return null;
            var request = new CouchRequest(baseUri + "_session");
            request.GetRequest().Headers.Add("Authorization:Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + ":" + password)));
            using (HttpWebResponse response = request.Post()
                .Form()
                .Data("name=" + username + "&password=" + password)
                .GetHttpResponse())
            {

                var header = response.Headers.Get("Set-Cookie");
                if (header != null)
                {
                    var parts = header.Split(';')[0].Split('=');
                    authCookie = new Cookie(parts[0], parts[1]);
                    authCookie.Domain = response.Server;
                    cookiestore.Add("authcookie", authCookie, TimeSpan.FromMinutes(9));
                }
                return authCookie;
            }
        }

        public void SetTimeout(int timeoutMs)
        {
            timeout = timeoutMs;
        }

        protected CouchRequest GetRequest(string uri)
        {
            return GetRequest(uri, null);
        }

        protected CouchRequest GetRequest(string uri, string etag)
        {
            CouchRequest request;
            if (AuthenticationType.Cookie == this.authType)
            {
                request = new CouchRequest(uri, GetSession(), etag);
            }
            else if (AuthenticationType.Basic == this.authType) //Basic Authentication
            {
                request = new CouchRequest(uri, username, password);
            }
            else //default Cookie
            {
                request = new CouchRequest(uri, GetSession(), etag);
            }
            if (timeout.HasValue) request.Timeout(timeout.Value);
            return request;
        }




    }
}