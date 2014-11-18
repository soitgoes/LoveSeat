using System;
using System.Net;

namespace LoveSeat.Support
{
	public enum DbType
	{
		CouchDb,
		Cloudant
	}
    public abstract class CouchBase
    {
        //protected readonly string username;
        //protected readonly string password;
        //protected readonly AuthenticationType authType;
        //protected string baseUri;
        private TtlDictionary<string, Cookie> cookiestore = new TtlDictionary<string, Cookie>();
        //private int? timeout;
        //private Cookie sessionCookie;
        public static log4net.ILog Logger = log4net.LogManager.GetLogger("LoveSeat");
        //protected DbType dbType;
        protected ICouchConnection couchConnection;
        private ICouchFactory couchFactory = new CouchFactory();

        protected CouchBase()
        {
            throw new Exception("Should not be used.");
        }
        protected CouchBase(ICouchConnection couchConnection)
        {
            this.couchConnection = couchConnection;

            if (couchConnection.AuthenticationType == AuthenticationType.Cookie)
                couchConnection.SessionCookie = GetSession();
        }

        public static bool Authenticate(string baseUri, string userName, string password)
        {
            if (!baseUri.Contains("http://"))
                baseUri = "http://" + baseUri;
            var request = new CouchRequest(new Uri(baseUri + "/_session"));
            var response = request.Post()
                .ContentType("application/x-www-form-urlencoded")
                .Data("name=" + userName + "&password=" + password)
                .Timeout(3000)
                .GetCouchResponse();
            return response.StatusCode == HttpStatusCode.OK;
        }

        public Cookie GetSession()
        {
            var authCookie = cookiestore["authcookie"];

            if (authCookie != null && !authCookie.Expired)
                return authCookie;

            if (string.IsNullOrEmpty(couchConnection.Username)) return null;
            var request = new CouchRequest(couchConnection.BaseUri.Combine("_session"));
            request.GetRequest().Headers.Add("Authorization:Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(couchConnection.Username + ":" + couchConnection.Password)));
            using (HttpWebResponse response = request.Post()
                .Form()
                .Data("name=" + couchConnection.Username + "&password=" + couchConnection.Password)
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

        public bool HasSessionExpired
        {
            get
            {
                return this.GetSession().Expired;
            }
            
        }

        public void SetTimeout(int timeoutMs)
        {
            couchConnection.Timeout = timeoutMs;
        }


        protected ICouchRequest GetRequest(Uri uri)
        {
            return GetRequest(uri, null);
        }

        protected ICouchRequest GetRequest(Uri uri, string etag)
        {
            return couchFactory.GetRequest(uri, etag, couchConnection);

        }




    }
}