using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace LoveSeat.Support
{
    public interface ICouchRequest
    {
        ICouchRequest Put();
        ICouchRequest Copy();
        ICouchRequest Get();
        ICouchRequest Post();
        ICouchRequest Delete();
        ICouchRequest Data(Stream data);
        ICouchRequest Data(string data);
        ICouchRequest Data(byte[] attachment);
        ICouchRequest Data(JObject obj);
        ICouchRequest ContentType(string contentType);
        ICouchRequest Form();
        ICouchRequest Json();
        ICouchRequest Timeout(int timeoutMs);
        HttpWebRequest GetRequest();

        /// <summary>
        /// Get the response from CouchDB.
        /// </summary>
        /// <returns></returns>
        ICouchResponse GetCouchResponse();

        HttpWebResponse GetHttpResponse();
        Task<ICouchResponse> GetCouchResponseAsync();
    }

    /// <summary>
    /// Repersent a web request for CouchDB database.
    /// </summary>
    public class CouchRequest : ICouchRequest
    {
        private const string INVALID_USERNAME_OR_PASSWORD = "reason=Name or password is incorrect";
        private const string NOT_AUTHORIZED = "reason=You are not authorized to access this db.";
        private const int STREAM_BUFFER_SIZE = 4096;

        private readonly HttpWebRequest request;
        public CouchRequest(Uri uri)
            : this(uri, new Cookie(), null)
        {
        }

        /// <summary>
        /// Request with Cookie authentication
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="authCookie"></param>
        /// <param name="eTag"></param>
        public CouchRequest(Uri uri, Cookie authCookie, string eTag)
        {
            request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Clear(); //important
            if (!string.IsNullOrEmpty(eTag))
                request.Headers.Add("If-None-Match", eTag);
            request.Headers.Add("Accept-Charset", "utf-8");
            request.Headers.Add("Accept-Language", "en-us");
            request.Accept = "application/json";
            request.Referer = uri.ToString();
            request.ContentType = "application/json";
            request.KeepAlive = true;
            if (authCookie != null)
                request.Headers.Add("Cookie", "AuthSession=" + authCookie.Value);
            request.Timeout = 10000;

			//Logger.DebugFormat("Request cookie: {0}, issued: {1}, expires: {2}, expired: {3}", authCookie.Name, authCookie.TimeStamp.ToString("hh:mm:ss,FFF zzz"), authCookie.Expires, authCookie.Expired);
        }

        /// <summary>
        /// Basic Authorization Header
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public CouchRequest(Uri uri, string username, string password) {

            request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Clear(); //important

            // Deal with Authorization Header
            if (username != null)
            {
                string authValue = "Basic ";
                string userNAndPassword = username + ":" + password;

                // Base64 encode
                string b64 = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(userNAndPassword));

                authValue = authValue + b64;

                request.Headers.Add("Authorization", authValue);
            }

            request.Headers.Add("Accept-Charset", "utf-8");
            request.Headers.Add("Accept-Language", "en-us");
            request.ContentType = "application/json";
            request.KeepAlive = true;
            request.Timeout = 10000;
        }

        public ICouchRequest Put() 
        {
            request.Method = "PUT";
            return this;
        }

        public ICouchRequest Copy()
		{
			request.Method = "COPY";
			return this;
		}

        public ICouchRequest Get()
        {
            request.Method = "GET";
            return this;
        }
        public ICouchRequest Post()
        {
            request.Method = "POST";
            return this;
        }
        public ICouchRequest Delete()
        {
            request.Method = "DELETE";
            return this;
        }
        public ICouchRequest Data(Stream data)
        {
            using (var body = request.GetRequestStream())
            {
                var buffer = new byte[STREAM_BUFFER_SIZE];
                var bytesRead = 0;
                while (0 != (bytesRead = data.Read(buffer, 0, buffer.Length)))
                {
                    body.Write(buffer, 0, bytesRead);
                }
            }
            return this;
        }
        public ICouchRequest Data(string data)
        {
            using (var body = request.GetRequestStream())
            {
                var encodedData = Encoding.UTF8.GetBytes(data);
                body.Write(encodedData, 0, encodedData.Length);
            }
            return this;
        }
        public ICouchRequest Data(byte[] attachment)
        {
            using (var body = request.GetRequestStream())
            {
                body.Write(attachment, 0, attachment.Length);
            }
            return this;
        }
        public ICouchRequest Data(JObject obj)
        {
            return Data(obj.ToString());
        }

        public ICouchRequest ContentType(string contentType)
        {
            request.ContentType = contentType;
            return this;
        }

        public ICouchRequest Form()
        {
            request.ContentType = "application/x-www-form-urlencoded";
            return this;
        }

        public ICouchRequest Json()
        {
            request.ContentType = "application/json";
            return this;
        }

        public ICouchRequest Timeout(int timeoutMs)
        {
            request.Timeout = timeoutMs;
            return this;
        }

        public HttpWebRequest GetRequest()
        {
            return request;
        }


        public async Task<ICouchResponse> GetCouchResponseAsync()
        {
            bool failedAuth = false;
            try
            {
                CouchBase.Logger.DebugFormat("GetCouchResponse {1} {0}", request.RequestUri, request.Method);
                using (var response = (HttpWebResponse)(await request.GetResponseAsync()))
                {
                    string msg = "";
                    if (isAuthenticateOrAuthorized(response, ref msg) == false)
                    {
                        failedAuth = true;
                        throw new WebException(msg);
                    }

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        throw new CouchException(request, response, response.GetResponseString() + "\n" + request.RequestUri);
                    }

                    return new CouchResponse(response);
                }
            }
            catch (WebException webEx)
            {
                if (failedAuth == true)
                {
                    throw;
                }
                var response = (HttpWebResponse)webEx.Response;
                if (response == null)
                    throw new HttpException("Request failed to receive a response", webEx);
                return new CouchResponse(response);
            }
            throw new HttpException("Request failed to receive a response");
        }

        /// <summary>
        /// Get the response from CouchDB.
        /// </summary>
        /// <returns></returns>
        public ICouchResponse GetCouchResponse()
        {

            bool failedAuth = false;
            try
            {
                CouchBase.Logger.DebugFormat("GetCouchResponse {1} {0}", request.RequestUri, request.Method);
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    string msg = "";
                    if (isAuthenticateOrAuthorized(response, ref msg) == false)
                    {
                        failedAuth = true;
                        throw new WebException(msg);
                    }

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        throw new CouchException(request, response, response.GetResponseString() + "\n" + request.RequestUri);
                    }

                    return new CouchResponse(response);
                }
            }
            catch (WebException webEx)
            {
                if (failedAuth == true)
                {
                    throw;
                }
                var response = (HttpWebResponse)webEx.Response;
                if (response == null)
                    throw new HttpException("Request failed to receive a response", webEx);
                return new CouchResponse(response);
            }
            throw new HttpException("Request failed to receive a response");
        }

        public HttpWebResponse GetHttpResponse()
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
                    throw;
                }
                var response = (HttpWebResponse)webEx.Response;
                if (response == null)
                    throw new HttpException("Request failed to receive a response. " + webEx.Message );
                return response;
            }
            throw new HttpException("Request failed to receive a response");
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