using System.Net;
using LoveSeat.Support;

namespace LoveSeat
{
   public  class CouchException : System.Exception
    {
       private readonly HttpWebRequest request;
       private readonly HttpWebResponse response;

       public CouchException(HttpWebRequest request, HttpWebResponse response, string mesg) : base(mesg)
       {
           this.request = request;
           this.response = response;
       }

       public CouchException(HttpWebRequest request, CouchResponse response, string mesg)
           : base(mesg)
       {
       }

       public HttpWebRequest Request { get { return request; } }
       public HttpWebResponse Response { get { return response; } }
    }
}
