using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
    public class CouchConfiguration
    {
        public CouchConfiguration()
        {
        }
        public CouchConfiguration(string pathToCouchAppRc)
        {
            var fileContents = File.ReadAllText(pathToCouchAppRc);
            var jboj = JObject.Parse(fileContents);
            var tmp= jboj["env"]["default"].Value<string>("db");
            var uri = new Uri(tmp);
            Host = uri.Host;
            Port = uri.Port;
            var userInfo = uri.UserInfo.Split(':');
            User = userInfo[0];
            Password = userInfo[1];
            Database = uri.PathAndQuery;
        }

        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string Host { get; set; }
    }
}