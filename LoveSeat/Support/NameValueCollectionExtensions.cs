using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace LoveSeat.Support
{
    public static class NameValueCollectionExtensions
    {
        public static string ToQueryString(this NameValueCollection collection)
        {
            var sb = new StringBuilder();

            bool first = true;

            foreach (string key in collection.AllKeys)
            {
                foreach (string value in collection.GetValues(HttpUtility.UrlEncode(key)))
                {
                    if (!first)
                    {
                        sb.Append("&");
                    }

                    sb.AppendFormat("{0}={1}", Uri.EscapeDataString(key), Uri.EscapeDataString(value));

                    first = false;
                }
            }

            return sb.ToString();
        }

        public static NameValueCollection AddReturn(this NameValueCollection target, NameValueCollection addFrom)
        {
            target.Add(addFrom);
            return target;
        }
    }
}