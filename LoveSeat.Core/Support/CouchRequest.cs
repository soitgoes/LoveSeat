using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace LoveSeat.Core.Support
{
    /// <summary>
    /// Repersent a web request for CouchDB database.
    /// </summary>
    public class CouchRequest
    {
        private const string INVALID_USERNAME_OR_PASSWORD = "reason=Name or password is incorrect";
        private const string NOT_AUTHORIZED = "reason=You are not authorized to access this db.";
        private const int STREAM_BUFFER_SIZE = 4096;

        private readonly HttpWebRequest _request;

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
            _request = (HttpWebRequest)WebRequest.Create(uri);

            if (!string.IsNullOrEmpty(eTag))
                _request.Headers[HttpRequestHeader.IfNoneMatch] = eTag;

            _request.Headers[HttpRequestHeader.AcceptCharset] = "utf-8";
            _request.Headers[HttpRequestHeader.AcceptLanguage] = "en-us";
            _request.Headers[HttpRequestHeader.KeepAlive] = "true";
            _request.ContentType = "application/json";

            if (authCookie != null)
                _request.Headers[HttpRequestHeader.Cookie] = "AuthSession=" + authCookie.Value;
        }

        /// <summary>
        /// Basic Authorization Header
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public CouchRequest(string uri, string username, string password)
        {
            _request = (HttpWebRequest)WebRequest.Create(uri);

            // Deal with Authorization Header
            if (username != null)
            {
                string auth = string.Format("{0}:{1}", username, password);
                string enc = Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));
                string cred = string.Format("{0} {1}", "Basic", enc);

                _request.Headers[HttpRequestHeader.Authorization] = cred;
            }

            _request.Headers[HttpRequestHeader.AcceptCharset] = "utf-8";
            _request.Headers[HttpRequestHeader.AcceptLanguage] = "en-us";
            _request.Headers[HttpRequestHeader.KeepAlive] = "true";
            _request.ContentType = "application/json";
            _request.Accept = "application/json";
        }

        public CouchRequest Put()
        {
            _request.Method = "PUT";
            return this;
        }

        public CouchRequest Get()
        {
            _request.Method = "GET";
            return this;
        }

        public CouchRequest Post()
        {
            _request.Method = "POST";
            return this;
        }

        public CouchRequest Delete()
        {
            _request.Method = "DELETE";
            return this;
        }

        public CouchRequest Data(Stream data)
        {
            return DataAsync(data).GetAwaiter().GetResult();
        }

        public async Task<CouchRequest> DataAsync(Stream data)
        {
            using (var body = await _request.GetRequestStreamAsync().ConfigureAwait(false))
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

        public CouchRequest Data(string data)
        {
            return DataAsync(data).GetAwaiter().GetResult();
        }

        public async Task<CouchRequest> DataAsync(string data)
        {
            using (var body = await _request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                var encodedData = Encoding.UTF8.GetBytes(data);
                body.Write(encodedData, 0, encodedData.Length);
            }

            return this;
        }

        public CouchRequest Data(byte[] attachment)
        {
            DataAsync(attachment).GetAwaiter().GetResult();
            return this;
        }

        public async Task<CouchRequest> DataAsync(byte[] attachment)
        {
            using (var body = await _request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                body.Write(attachment, 0, attachment.Length);
            }

            return this;
        }

        public CouchRequest Data(JObject obj)
        {
            return Data(obj.ToString());
        }

        public Task<CouchRequest> DataAsync(JObject obj)
        {
            return DataAsync(obj.ToString());
        }

        public CouchRequest ContentType(string contentType)
        {
            _request.ContentType = contentType;
            return this;
        }

        public CouchRequest Form()
        {
            _request.ContentType = "application/x-www-form-urlencoded";
            return this;
        }

        public CouchRequest Json()
        {
            _request.ContentType = "application/json";
            return this;
        }

        public CouchRequest Timeout(int? timeout)
        {
            if (timeout.HasValue)
                _request.ContinueTimeout = timeout.Value;
            else
                _request.ContinueTimeout = (int)TimeSpan.FromHours(1).TotalMilliseconds;

            return this;
        }

        public HttpWebRequest GetRequest()
        {
            return _request;
        }

        /// <summary>
        /// Get the response from CouchDB.
        /// </summary>
        /// <returns></returns>
        public CouchResponse GetCouchResponse()
        {
            return GetCouchResponseAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get the response from CouchDB.
        /// </summary>
        /// <returns></returns>
        public async Task<CouchResponse> GetCouchResponseAsync()
        {
            bool failedAuth = false;

            try
            {
                HttpWebResponse response = (HttpWebResponse)await _request.GetResponseAsync().ConfigureAwait(false);

                using (response)
                {
                    string msg = string.Empty;
                    if (isAuthenticateOrAuthorized(response, ref msg) == false)
                    {
                        failedAuth = true;
                        throw new WebException(msg);
                    }

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        throw new CouchException(_request, response, response.GetResponseString() + "\n" + _request.RequestUri);
                    }

                    return new CouchResponse(response);
                }
            }
            catch (WebException ex)
            {
                if (failedAuth == true)
                    throw ex;

                var response = (HttpWebResponse)ex.Response;
                if (response == null)
                    throw new Exception("Request failed to receive a response", ex);

                return new CouchResponse(response);
            }

            throw new Exception("Request failed to receive a response");
        }

        public HttpWebResponse GetHttpResponse()
        {
            return GetHttpResponseAsync().GetAwaiter().GetResult();
        }

        public async Task<HttpWebResponse> GetHttpResponseAsync()
        {
            bool failedAuth = false;

            try
            {
                HttpWebResponse response = (HttpWebResponse)await _request.GetResponseAsync().ConfigureAwait(false);

                string msg = string.Empty;
                if (isAuthenticateOrAuthorized(response, ref msg) == false)
                {
                    failedAuth = true;
                    throw new WebException(msg, new Exception(msg));
                }

                return response;
            }
            catch (WebException ex)
            {
                if (failedAuth == true)
                    throw;

                var response = (HttpWebResponse)ex.Response;
                if (response == null)
                    throw new Exception("Request failed to receive a response", ex);

                return response;
            }

            throw new Exception("Request failed to receive a response");
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
                    string temp = WebUtility.UrlDecode(split[i]);

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
        }
    }
}