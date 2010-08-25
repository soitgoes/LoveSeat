using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
	public static class JObjectExtensionMethods
	{
		public static string  Id (this JObject obj)
		{
			string s = obj["_id"].ToString();
			return s.Replace("\"","");
		}

		public static string Rev(this JObject obj)
		{
			string s = obj["_rev"].ToString();
			return s.Replace("\"", "");
		}
	}
}