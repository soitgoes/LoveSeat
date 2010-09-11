using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
	public class CouchDocument : JObject
	{
		public string Id { get { return this["_id"].Value<string>(); } }
		public string Rev { get { return this["_rev"].Value<string>();  } }

		public CouchDocument(JObject jobj) : base(jobj)
		{
		}
		public bool HasAttachment
		{
			get { return this["_attachments"] != null; }
		}

		public IEnumerable<string> GetAttachmentNames()
		{
			var attachment = this["_attachments"];
			if (attachment == null)  return null;
			return attachment.Select(x => x.Value<JProperty>().Name);
		}

	}
}