using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LoveSeat.Core.Support
{
    public abstract class CouchBase
    {
        private TtlDictionary<string, Cookie> _cookiestore = new TtlDictionary<string, Cookie>();

        protected readonly string _username;
        protected readonly string _password;
        protected AuthenticationType _authType;
        protected string _baseUri;

        protected int? _timeout;

        protected CouchBase()
        {
        }

        protected CouchBase(string username, string password, AuthenticationType authType)
        {
            _username = username;
            _password = password;
            _authType = authType;
        }

        public static bool Authenticate(string baseUri, string userName, string password)
        {
            return AuthenticateAsync(baseUri, userName, password).GetAwaiter().GetResult();
        }

        public static async Task<bool> AuthenticateAsync(string baseUri, string userName, string password)
        {
            if (!baseUri.Contains("http://"))
                baseUri = "http://" + baseUri;

            var request = new CouchRequest(baseUri + "/_session");

            var response = await  (await request.Post()
                .ContentType("application/x-www-form-urlencoded")
                .DataAsync($"name={userName}&password={password}")
                .ConfigureAwait(false))
                .Timeout(3000)
                .GetCouchResponseAsync()
                .ConfigureAwait(false);

            return response.StatusCode == HttpStatusCode.OK;
        }

        public Cookie GetSession()
        {
            return GetSessionAsync().GetAwaiter().GetResult();
        }

        public async Task<Cookie> GetSessionAsync()
        {
            var authCookie = _cookiestore["authcookie"];

            if (authCookie != null)
                return authCookie;

            if (string.IsNullOrEmpty(_username))
                return null;

            var request = new CouchRequest(_baseUri + "_session");
            request.GetRequest().Headers[HttpRequestHeader.Authorization] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(_username + ":" + _password));

            using (HttpWebResponse response = await (await request.Post()
                .Form()
                .DataAsync($"name={_username}&password={_password}")
                .ConfigureAwait(false))
                .GetHttpResponseAsync()
                .ConfigureAwait(false))
            {

                var header = response.Headers["Set-Cookie"];
                if (header != null)
                {
                    var parts = header.Split(';')[0].Split('=');
                    authCookie = new Cookie(parts[0], parts[1]);
                    _cookiestore.Add("authcookie", authCookie, TimeSpan.FromMinutes(9));
                }

                return authCookie;
            }
        }

        public void SetTimeout(int timeout)
        {
            _timeout = timeout;
        }

        protected CouchRequest GetRequest(string uri)
        {
            return GetRequest(uri, null);
        }

        protected Task<CouchRequest> GetRequestAsync(string uri)
        {
            return GetRequestAsync(uri, null);
        }

        protected CouchRequest GetRequest(string uri, string etag)
        {
            return GetRequestAsync(uri, etag).GetAwaiter().GetResult();
        }

        protected async Task<CouchRequest> GetRequestAsync(string uri, string etag)
        {
            CouchRequest request = null;
            if (AuthenticationType.Cookie == _authType)
            {
                request = new CouchRequest(uri, await GetSessionAsync().ConfigureAwait(false), etag);
            }
            else if (AuthenticationType.Basic == _authType) //Basic Authentication
            {
                request = new CouchRequest(uri, _username, _password);
            }
            else //default Cookie
            {
                request = new CouchRequest(uri, await GetSessionAsync().ConfigureAwait(false), etag);
            }

            if (_timeout.HasValue)
                request.Timeout(_timeout.Value);

            return request;
        }
    }
}