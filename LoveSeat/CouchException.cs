using System.Net;
using LoveSeat.Support;

namespace LoveSeat
{
   public  class CouchException : System.Exception
    {
       private readonly HttpWebRequest request;
       private readonly HttpWebResponse response;

       public CouchException(HttpWebRequest request, HttpWebResponse response, string mesg)
         : base(BuildExceptionMessage(mesg, request))
       {
           this.request = request;
           this.response = response;
       }

       public CouchException(HttpWebRequest request, CouchResponse response, string mesg)
         : base(BuildExceptionMessage(mesg, request))
       {
       }

       public HttpWebRequest Request { get { return request; } }
       public HttpWebResponse Response { get { return response; } }

       private static string BuildExceptionMessage(string msg, HttpWebRequest request) 
       {
         string excpetionMsg = string.Format("{0} {1}", request.RequestUri, msg);
         return excpetionMsg;
       }
    }
}
