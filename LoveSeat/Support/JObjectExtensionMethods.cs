using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoveSeat.Support
{
	public static class JObjectExtensionMethods
	{
		public static string  Id (this JObject obj)
		{
			return  obj["_id"].Value<string>();
		}

		public static string Rev(this JObject obj)
		{
			return obj["_rev"].Value<string>();
		}
	}
}