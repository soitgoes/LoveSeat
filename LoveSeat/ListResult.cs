using System;
using System.Net;
using LoveSeat.Interfaces;
using LoveSeat.Support;

namespace LoveSeat
{
    public class ListResult : IListResult
    {
        private readonly HttpWebRequest request;
        private readonly HttpWebResponse response;

        public ListResult(HttpWebRequest request , HttpWebResponse response)
        {
            this.request = request;
            this.response = response;
        }
        
        public HttpWebRequest Request
        {
            get { return request; }
        }

        public HttpWebResponse Response
        {
            get { return response; }
        }

        public HttpStatusCode StatusCode
        {
            get { return Response.StatusCode; }
        }

        public string Etag
        {
            get { return Response.Headers["ETag"]; }
        }

        public string RawString
        {
            get { return Response.GetResponseString(); }
        }

        public bool Equals(IListResult other)
        {
            if (other == null)
                return false;

            if (string.IsNullOrEmpty(other.Etag))
                return false;

            return other.Etag == Etag;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IListResult);
        }

        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(Etag))
                return base.GetHashCode();

            return Etag.GetHashCode();
        }
    }
}