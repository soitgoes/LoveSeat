using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
	public class CouchUser : Document
	{
		public CouchUser(JObject jobj)
			: base(jobj)
		{
		}

		public IEnumerable<string> Roles
		{
			get
			{
				if (!this["roles"].HasValues)
				{
					yield return null;
				}
				foreach (var role in this["roles"].Values())
				{
					yield return role.Value<string>();
				}
			}
		}
	}
}