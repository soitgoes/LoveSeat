using System.Net;

namespace LoveSeat.Interfaces
{
    public interface IListResult: System.IEquatable<IListResult>
    {
        /// <summary>
        /// Typically won't be needed Provided for debuging assitance
        /// </summary>
        HttpWebRequest Request { get; }

        HttpStatusCode StatusCode { get; }
        string Etag { get; }
        string RawString { get; }

    }
}