using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace LoveSeat.Support
{
    public class CouchRequest
    {
        private const string INVALID_USERNAME_OR_PASSWORD = "reason=Name or password is incorrect";
        private const string NOT_AUTHORIZED = "reason=You are not authorized to access this db.";

        private readonly HttpWebRequest request;
        public CouchRequest(string uri)
            : this(uri, new Cookie(), null)
        {
        }

        /// <summary>
        /// Request with Cookie authentication
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="authCookie"></param>
        /// <param name="eTag"></param>
        public CouchRequest(string uri, Cookie authCookie, string eTag)
        {
            request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Clear(); //important
            if (!string.IsNullOrEmpty(eTag))
                request.Headers.Add("If-None-Match", eTag);
            request.Headers.Add("Accept-Charset", "utf-8");
            request.Headers.Add("Accept-Language", "en-us");
            request.Referer = uri;
            request.ContentType = "application/json";
            request.KeepAlive = true;
            if (authCookie != null)
                request.Headers.Add("Cookie", "AuthSession=" + authCookie.Value);
            request.Timeout = Timeout.HasValue ? Timeout.Value : 10000;
        }

        /// <summary>
        /// Basic Authorization Header
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public CouchRequest(string uri, string username, string password)
        {

            request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Clear(); //important

            // Deal with Authorization Header
            string authValue = "Basic ";
            string userNAndPassword = username + ":" + password;

            // Base64 encode
            string b64 = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(userNAndPassword));

            authValue = authValue + b64;

            request.Headers.Add("Authorization", authValue);
            request.Headers.Add("Accept-Charset", "utf-8");
            request.Headers.Add("Accept-Language", "en-us");
            request.Referer = uri;
            request.ContentType = "application/json";
            request.KeepAlive = true;
        }


        public int? Timeout { get; set; }
        public CouchRequest Put()
        {
            request.Method = "PUT";
            return this;
        }

        public CouchRequest Get()
        {
            request.Method = "GET";
            return this;
        }
        public CouchRequest Post()
        {
            request.Method = "POST";
            return this;
        }
        public CouchRequest Delete()
        {
            request.Method = "DELETE";
            return this;
        }
        public CouchRequest Data(string data)
        {
            using (var body = request.GetRequestStream())
            {
                var encodedData = Encoding.UTF8.GetBytes(data);
                body.Write(encodedData, 0, encodedData.Length);
            }
            return this;
        }
        public CouchRequest Data(byte[] attachment)
        {
            using (var body = request.GetRequestStream())
            {
                body.Write(attachment, 0, attachment.Length);
            }
            return this;
        }
        public CouchRequest Data(JObject obj)
        {
            return Data(obj.ToString());
        }

        public CouchRequest ContentType(string contentType)
        {
            request.ContentType = contentType;
            return this;
        }

        public CouchRequest Form()
        {
            request.ContentType = "application/x-www-form-urlencoded";
            return this;
        }

        public CouchRequest Json()
        {
            request.ContentType = "application/json";
            return this;
        }

        public HttpWebRequest GetRequest()
        {
            return request;
        }

        public HttpWebResponse GetResponse()
        {
            bool failedAuth = false;
            try
            {
                var response = (HttpWebResponse)request.GetResponse();

                string msg = "";
                if (isAuthenticateOrAuthorized(response, ref msg) == false)
                {
                    failedAuth = true;
                    throw new WebException(msg, new System.Exception(msg));
                }

                return response;
            }
            catch (WebException webEx)
            {
                if (failedAuth == true)
                {
                    throw webEx;
                }
                var response = (HttpWebResponse)webEx.Response;
                if (response != null)
                {
                    return response;
                }
            }
            return null;
        }


        /// <summary>
        /// Checks response if username and password was valid
        /// </summary>
        /// <param name="response"></param>
        private bool isAuthenticateOrAuthorized(HttpWebResponse response, ref string message)
        {
            //"reason=Name or password is incorrect"
            // Check if query string is okay
            string[] split = response.ResponseUri.Query.Split('&');

            if (split.Length > 0)
            {
                for (int i = 0; i < split.Length; i++)
                {
                    string temp = System.Web.HttpUtility.UrlDecode(split[i]);

                    if (temp.Contains(INVALID_USERNAME_OR_PASSWORD) == true)
                    {
                        message = "Invalid username or password";
                        return false;
                    }
                    else if (temp.Contains(NOT_AUTHORIZED) == true)
                    {
                        message = "Not Authorized to access database";
                        return false;
                    }
                }
            }
            return true;
        }// end private void checkResponse(...


    }
}