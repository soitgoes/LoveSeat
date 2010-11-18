using System;
using System.IO;
using System.Web;
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
		/// <param name="id">Id of Document</param>
		/// <param name="jsonForDocument"></param>
		/// <returns></returns>
		public CouchDocument CreateDocument(string id, string jsonForDocument)
		{
			return GetRequest(databaseBaseUri +"/" +  id)
				.Put().Form()
				.Data(jsonForDocument)
				.GetResponse()
				.GetCouchDocument();
		}

        public CouchDocument CreateDocument(CouchDocument doc)
        {
            return CreateDocument(doc.Id, doc.ToString());
        }
		/// <summary>
		/// Creates a document when you intend for Couch to generate the id for you.
		/// </summary>
		/// <param name="jsonForDocument">Json for creating the document</param>
		/// <returns></returns>
		public CouchDocument CreateDocument(string jsonForDocument)
		{
			return
				GetRequest(databaseBaseUri + "/").Post().Json().Data(jsonForDocument).GetResponse().GetCouchDocument();
		}
		public JObject DeleteDocument(string id, string rev)
		{
			return GetRequest(databaseBaseUri + "/" + id + "?rev=" + rev).Delete().Form().GetResponse().GetJObject();
		}
		/// <summary>
		/// Returns null if document is not found
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public CouchDocument GetDocument(string id)
		{
			try
			{
				return GetRequest(databaseBaseUri + "/" + id).Get().Json().GetResponse().GetCouchDocument();	
			}catch(CouchException ce)
			{
				if (ce.Message.Contains("not_found"))
				{
					return null;
				}
				throw;
			}
		}
		/// <summary>
		/// Adds an attachment to a document.  If revision is not specified then the most recent will be fetched and used.  Warning: if you need document update conflicts to occur please use the method that specifies the revision
		/// </summary>
		/// <param name="id">id of the couch Document</param>
		/// <param name="attachment">byte[] of of the attachment.  Use File.ReadAllBytes()</param>
		/// <param name="contentType">Content Type must be specifed</param>	
		public JObject AddAttachment(string id, byte[] attachment, string filename, string contentType)
		{
			var doc = GetDocument(id);
			return AddAttachment(id, doc.Rev, attachment,filename, contentType);
		}
		/// <summary>
		/// Adds an attachment to the documnet.  Rev must be specified on this signature.  If you want to attach no matter what then use the method without the rev param
		/// </summary>
		/// <param name="id">id of the couch Document</param>
		/// <param name="rev">revision _rev of the Couch Document</param>
		/// <param name="attachment">byte[] of of the attachment.  Use File.ReadAllBytes()</param>
		/// <param name="filename">filename of the attachment</param>
		/// <param name="contentType">Content Type must be specifed</param>			
		/// <returns></returns>
		public JObject AddAttachment(string id, string rev,  byte[] attachment,string filename, string contentType)
		{
			return
				GetRequest(databaseBaseUri + "/" + id + "/" +filename + "?rev=" + rev).Put().ContentType(contentType).Data(attachment).GetResponse().GetJObject();
		}

		public Stream GetAttachmentStream(CouchDocument doc, string attachmentName)
		{
			return GetAttachmentStream(doc.Id, doc.Rev, attachmentName);
		}
		public Stream GetAttachmentStream(string docId, string rev, string attachmentName)
		{
			return GetRequest(databaseBaseUri + "/" + docId + "/" + HttpUtility.UrlEncode(attachmentName)).Get().GetResponse().GetResponseStream();
		}
		public Stream GetAttachmentStream(string docId, string attachmentName)
		{
			var doc = GetDocument(docId);
			if (doc == null) return null;
			return GetAttachmentStream(docId, doc.Rev, attachmentName);
		}
		public JObject DeleteAttachment(string id, string rev, string attachmentName)
		{
			return GetRequest(databaseBaseUri + "/" + id + "/" + attachmentName + "?rev=" + rev).Json().Delete().GetResponse().GetJObject();			
		}
		public JObject DeleteAttachment(string id, string attachmentName)
		{
			var doc = GetDocument(id);
			return DeleteAttachment(doc.Id, doc.Rev, attachmentName);
		}

	    public CouchDocument SaveDocument(CouchDocument document)
	    {
            if (document.Rev == null)
                return CreateDocument(document);

	        var resp =  GetRequest(databaseBaseUri + "/" + document.Id + "?rev="+ document.Rev).Put().Form().Data(document).GetResponse();
	        var jobj = resp.GetJObject();
	       //TODO: Change this so it simply alters the revision on the document past in so that there isn't an additional request.
            return GetDocument(document.Id);
	    }
	}
}