using System;
using LoveSeat.Interfaces;

namespace LoveSeat.Support
{
    public interface ICouchFactory
    {
        IDocumentDatabase GetDatabase(ICouchConnection couchConnection);
        ICouchRequest GetRequest(Uri uri, ICouchConnection couchConnection);
        ICouchRequest GetRequest(Uri uri, string etag, ICouchConnection couchConnection);
        ICouchClient GetClient(ICouchConnection couchConnection); 
    }
}