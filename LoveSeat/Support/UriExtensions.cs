using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveSeat.Support
{
    public static class UriExtensions
    {
        /// <summary>
        /// Will add path to the Uri
        /// </summary>
        /// <param name="thisUri"></param>
        /// <param name="path">If this is just a querystring then start it with a ?</param>
        /// <returns></returns>
        public static Uri Combine(this Uri thisUri, string path)
        {
            var thisUriString = thisUri.ToString();

            //just a query string then append it
            if (path.StartsWith("?"))
                return new Uri(thisUriString.TrimEnd('/') + path);

            //if path starts with / remove it otherwise we may get double
            if (path.StartsWith("/"))
                path = path.TrimStart('/');

            //if uri does not end in / then add it otherwise we end up off the root
            if (!thisUriString.EndsWith("/"))
                thisUriString = thisUriString + "/";


            return new Uri(thisUriString + path);
        }
    }
}
