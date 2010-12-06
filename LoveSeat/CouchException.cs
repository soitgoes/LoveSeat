using System.Net;

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

       public HttpWebRequest Request { get { return request; } }
       public HttpWebResponse Response { get { return response; } }
    }
}
