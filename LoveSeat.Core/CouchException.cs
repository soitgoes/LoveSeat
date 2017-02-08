using LoveSeat.Core.Support;
using System;
using System.Net;

namespace LoveSeat.Core
{
    public class CouchException : Exception
    {
        private readonly HttpWebRequest _request;
        private readonly HttpWebResponse _response;

        public CouchException(HttpWebRequest request, HttpWebResponse response, string mesg)
          : base(BuildExceptionMessage(mesg, request))
        {
            _request = request;
            _response = response;
        }

        public CouchException(HttpWebRequest request, CouchResponse response, string mesg)
          : base(BuildExceptionMessage(mesg, request))
        {
        }

        public HttpWebRequest Request
        {
            get { return _request; }
        }

        public HttpWebResponse Response
        {
            get { return _response; }
        }

        private static string BuildExceptionMessage(string msg, HttpWebRequest request)
        {
            string excpetionMsg = string.Format("{0} {1}", request.RequestUri, msg);
            return excpetionMsg;
        }
    }
}
