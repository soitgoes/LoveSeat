using System.Net;
using LoveSeat.Core.Interfaces;
using LoveSeat.Core.Support;

namespace LoveSeat.Core
{
    public class ListResult : IListResult
    {
        private readonly HttpWebRequest _request;
        private readonly CouchResponse _response;

        public ListResult(HttpWebRequest request, CouchResponse response)
        {
            _request = request;
            _response = response;
        }

        public HttpWebRequest Request
        {
            get { return _request; }
        }

        public HttpStatusCode StatusCode
        {
            get { return _response.StatusCode; }
        }

        public string Etag
        {
            get { return _response.ETag; }
        }

        public string RawString
        {
            get { return _response.ResponseString; }
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