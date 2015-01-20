using System.Collections.Generic;
using System.Net;
using LoveSeat.Support;
using Newtonsoft.Json.Linq;

namespace LoveSeat.Interfaces
{
    public interface IViewResult<T> : IViewResult
    {
        CouchDictionary<T> Dictionary { get; }
        IEnumerable<T> Items { get; }
    }

    public interface IViewResult : System.IEquatable<IListResult>
    {
        JObject Json { get; }

        /// <summary>
        /// Typically won't be needed.  Provided for debuging assistance
        /// </summary>
        HttpWebRequest Request { get; }

        /// <summary>
        /// Typically won't be needed.  Provided for debugging assistance
        /// </summary>
        HttpWebResponse Response { get; }

        HttpStatusCode StatusCode { get; }
        string Etag { get; }
        int TotalRows { get; }
        int OffSet { get; }
        IEnumerable<JToken> Rows { get; }

        /// <summary>
        /// Only populated when IncludeDocs is true
        /// </summary>
        IEnumerable<JToken> Docs { get; }

        bool IncludeDocs { get; }

        /// <summary>
        /// An IEnumerable of strings insteda of the IEnumerable of JTokens
        /// </summary>
        IEnumerable<string> RawRows { get; }

        IEnumerable<string> RawValues { get; }
        IEnumerable<string> RawDocs { get; }
        string RawString { get; }

        /// <summary>
        /// Provides a formatted version of the json returned from this Result.  (Avoid this method in favor of RawString as it's much more performant)
        /// </summary>
        string FormattedResponse { get; }

        string ToString();
    }
}