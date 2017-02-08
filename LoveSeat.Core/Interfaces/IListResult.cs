using System;
using System.Net;

namespace LoveSeat.Core.Interfaces
{
    public interface IListResult: IEquatable<IListResult>
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