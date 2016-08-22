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
				if (!this.jObject["roles"].HasValues)
				{
					yield return null;
				}
				foreach (var role in this.jObject["roles"].Values())
				{
					yield return role.Value<string>();
				}
			}
		}
	}
}