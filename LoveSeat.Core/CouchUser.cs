using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace LoveSeat.Core
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
				if (!_jObject["roles"].HasValues)
                    yield return null;

                foreach (var role in _jObject["roles"].Values())
                    yield return role.Value<string>();
            }
		}
	}
}