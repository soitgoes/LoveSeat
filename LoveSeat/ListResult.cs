using System;
using System.Net;
using LoveSeat.Interfaces;
using LoveSeat.Support;

namespace LoveSeat
{
    public class ListResult : IListResult
    {
        private readonly HttpWebRequest request;
        private readonly ICouchResponse response;

        public ListResult(HttpWebRequest request, ICouchResponse response)
        {
            this.request = request;
            this.response = response;
        }
        
        public HttpWebRequest Request
        {
            get { return request; }
        }

        public ICouchResponse Response
        {
            get { return response; }
        }

        public HttpStatusCode StatusCode
        {
            get { return response.StatusCode; }
        }

        public string Etag
        {
            get { return response.ETag; }
        }

        public string RawString
        {
            get { return response.ResponseString; }
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