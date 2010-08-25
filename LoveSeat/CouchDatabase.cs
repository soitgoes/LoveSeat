using LoveSeat.Support;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
	public class CouchDatabase : CouchBase
	{
		private readonly string databaseBaseUri;

		public CouchDatabase(string baseUri, string databaseName,  string username, string password) : base(username, password)
		{
			this.baseUri = baseUri;
			this.databaseBaseUri = baseUri  + databaseName;
		}

		/// <summary>
		/// Creates a document using the json provided. 
		/// No validation or smarts attempted here by design for simplicities sake
		/// </summary>
		/// <param name="jsonForDocument"></param>
		/// <returns></returns>
		public JObject CreateDocument(string id, string jsonForDocument)
		{
			return GetRequest(databaseBaseUri +"/" +  id)
				.Put().Form()
				.Data(jsonForDocument)
				.GetResponse()
				.GetJObject();
		}
		public JObject DeleteDocument(string id, string rev)
		{
			return GetRequest(databaseBaseUri + "/" + id +"?rev=" + rev).Delete().Form().GetResponse().GetJObject();
		}
		/// <summary>
		/// Returns null if document is not found
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public JObject GetDocument(string id)
		{
			try
			{
				return GetRequest(databaseBaseUri + "/" + id).Get().Form().GetResponse().GetJObject();	
			}catch(CouchException ce)
			{
				if (ce.Message.Contains("not_found"))
				{
					return null;
				}
				throw;
			}
		}
		
	}
}