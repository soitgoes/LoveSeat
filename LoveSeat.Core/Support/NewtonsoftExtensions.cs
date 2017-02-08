using System.Linq;
using Newtonsoft.Json.Linq;

namespace LoveSeat.Core
{
    internal static class NewtonsoftExtensions
    {
        /// <summary>
        /// Please note this is a shallow overwrite with no real sophistication past the root object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        public static void CopyFromObj<T>(this JObject destination, T source) where T : class
        {
            var serializer = new DefaultSerializer();
            var json = serializer.Serialize(source);
            var jobj = JObject.Parse(json);

            foreach (var prop in jobj.Properties())
            {
                if (destination.Properties().Any(x => x.Name == prop.Name))
                    destination[prop.Name].Replace(jobj[prop.Name]);
                else
                    destination.Add(prop.Name, jobj[prop.Name]);
            }
        }
    }
}
