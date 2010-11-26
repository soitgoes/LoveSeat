using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using LoveSeat.Support;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
    public interface IDocumentDatabase
    {
        /// <summary>
        /// Creates a document using the json provided. 
        /// No validation or smarts attempted here by design for simplicities sake
        /// </summary>
        /// <param name="id">Id of Document</param>
        /// <param name="jsonForDocument"></param>
        /// <returns></returns>
        Document CreateDocument(string id, string jsonForDocument);

        Document CreateDocument(Document doc);

        /// <summary>
        /// Creates a document when you intend for Couch to generate the id for you.
        /// </summary>
        /// <param name="jsonForDocument">Json for creating the document</param>
        /// <returns></returns>
        Document CreateDocument(string jsonForDocument);

        JObject DeleteDocument(string id, string rev);

        /// <summary>
        /// Returns null if document is not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Document GetDocument(string id);

        T GetDocument<T>(string id);
        T GetDocument<T>(string id, IObjectSerializer<T> objectSerializer);

        /// <summary>
        /// Adds an attachment to a document.  If revision is not specified then the most recent will be fetched and used.  Warning: if you need document update conflicts to occur please use the method that specifies the revision
        /// </summary>
        /// <param name="id">id of the couch Document</param>
        /// <param name="attachment">byte[] of of the attachment.  Use File.ReadAllBytes()</param>
        /// <param name="contentType">Content Type must be specifed</param>	
        JObject AddAttachment(string id, byte[] attachment, string filename, string contentType);

        /// <summary>
        /// Adds an attachment to the documnet.  Rev must be specified on this signature.  If you want to attach no matter what then use the method without the rev param
        /// </summary>
        /// <param name="id">id of the couch Document</param>
        /// <param name="rev">revision _rev of the Couch Document</param>
        /// <param name="attachment">byte[] of of the attachment.  Use File.ReadAllBytes()</param>
        /// <param name="filename">filename of the attachment</param>
        /// <param name="contentType">Content Type must be specifed</param>			
        /// <returns></returns>
        JObject AddAttachment(string id, string rev, byte[] attachment, string filename, string contentType);

        Stream GetAttachmentStream(Document doc, string attachmentName);
        Stream GetAttachmentStream(string docId, string rev, string attachmentName);
        Stream GetAttachmentStream(string docId, string attachmentName);
        JObject DeleteAttachment(string id, string rev, string attachmentName);
        JObject DeleteAttachment(string id, string attachmentName);
        Document SaveDocument(Document document);

        /// <summary>
        /// Gets the results of a view with no view parameters.  Use the overload to pass parameters
        /// </summary>
        /// <param name="designDoc">The design doc on which the view resides</param>
        /// <param name="viewName">The name of the view</param>
        /// <returns></returns>
        ViewResult<T> View<T>(string designDoc, string viewName);

        /// <summary>
        /// Gets the results of the view using any and all parameters
        /// </summary>
        /// <param name="designDoc">The design doc on which the view resides</param>
        /// <param name="viewName">The name of the view</param>
        /// <param name="options">Options such as startkey etc.</param>
        /// <returns></returns>
        ViewResult<T> View<T>(string designDoc, string viewName, ViewOptions options);

        /// <summary>
        /// Don't use this overload unless you intend to override the default ObjectSerialization behavior.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="designDoc"></param>
        /// <param name="viewName"></param>
        /// <param name="options"></param>
        /// <param name="objectSerializer">Only needed unless you'd like to override the default behavior of the serializer</param>
        /// <returns></returns>
        ViewResult<T> View<T>(string designDoc, string viewName, ViewOptions options, IObjectSerializer<T> objectSerializer);

        /// <summary>
        /// Gets all the documents in the database using the _all_docs uri
        /// </summary>
        /// <returns></returns>
        ViewResult GetAllDocuments();

        ViewResult GetAllDocuments(ViewOptions options);
    }

    public class CouchDatabase : CouchBase, IDocumentDatabase
    {
        private readonly string databaseBaseUri;
         public CouchDatabase(string baseUri, string databaseName, string username, string password)
            : base(username, password)
        {
            this.baseUri = baseUri;
            this.databaseBaseUri = baseUri + databaseName;
        }

        /// <summary>
        /// Creates a document using the json provided. 
        /// No validation or smarts attempted here by design for simplicities sake
        /// </summary>
        /// <param name="id">Id of Document</param>
        /// <param name="jsonForDocument"></param>
        /// <returns></returns>
        public Document CreateDocument(string id, string jsonForDocument)
        {
            var resp = GetRequest(databaseBaseUri + "/" + id)
                .Put().Form()
                .Data(jsonForDocument)
                .GetResponse();
            return 
                resp.GetCouchDocument();
        }

        public Document CreateDocument(Document doc)
        {
            return CreateDocument(doc.Id, doc.ToString());
        }
        /// <summary>
        /// Creates a document when you intend for Couch to generate the id for you.
        /// </summary>
        /// <param name="jsonForDocument">Json for creating the document</param>
        /// <returns></returns>
        public Document CreateDocument(string jsonForDocument)
        {
            var json = JObject.Parse(jsonForDocument);
            var jobj = 
                GetRequest(databaseBaseUri + "/").Post().Json().Data(jsonForDocument).GetResponse().GetJObject();
            json["_id"] = jobj["_id"];
            json["_rev"] = jobj["_rev"];
            return new Document(json);
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
        public Document GetDocument(string id)
        {
            var resp = GetRequest(databaseBaseUri + "/" + id).Get().Json().GetResponse();
            if (resp.StatusCode==HttpStatusCode.NotFound) return null;
            return resp.GetCouchDocument();
        }

        public T GetDocument<T>(string id)
        {
            return GetDocument(id, new ObjectSerializer<T>());
        }
        public T GetDocument<T>(string id, IObjectSerializer<T> objectSerializer)
        {
            var resp = GetRequest(databaseBaseUri + "/" + id).Get().Json().GetResponse();
            if (resp.StatusCode == HttpStatusCode.NotFound) return default(T);
            return objectSerializer.Deserialize(resp.GetResponseString());
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
            return AddAttachment(id, doc.Rev, attachment, filename, contentType);
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
        public JObject AddAttachment(string id, string rev, byte[] attachment, string filename, string contentType)
        {
            return
                GetRequest(databaseBaseUri + "/" + id + "/" + filename + "?rev=" + rev).Put().ContentType(contentType).Data(attachment).GetResponse().GetJObject();
        }

        public Stream GetAttachmentStream(Document doc, string attachmentName)
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

        public Document SaveDocument(Document document)
        {
            if (document.Rev == null)
                return CreateDocument(document);

            var resp = GetRequest(databaseBaseUri + "/" + document.Id + "?rev=" + document.Rev).Put().Form().Data(document).GetResponse();
            var jobj = resp.GetJObject();
            //TODO: Change this so it simply alters the revision on the document past in so that there isn't an additional request.
            return GetDocument(document.Id);
        }

        /// <summary>
        /// Gets the results of a view with no view parameters.  Use the overload to pass parameters
        /// </summary>
        /// <param name="designDoc">The design doc on which the view resides</param>
        /// <param name="viewName">The name of the view</param>
        /// <returns></returns>
        public ViewResult<T> View<T>(string designDoc, string viewName)
        {
            return View<T>(designDoc, viewName, null);
        }

        /// <summary>
        /// Gets the results of the view using any and all parameters
        /// </summary>
        /// <param name="designDoc">The design doc on which the view resides</param>
        /// <param name="viewName">The name of the view</param>
        /// <param name="options">Options such as startkey etc.</param>
        /// <returns></returns>
        public ViewResult<T> View<T>(string designDoc, string viewName, ViewOptions options)
        {
            var uri = databaseBaseUri + "/_design/" + designDoc + "/_view/" + viewName;
            return ProcessGenericResults<T>(uri, options, new ObjectSerializer<T>());
        }
        /// <summary>
        /// Don't use this overload unless you intend to override the default ObjectSerialization behavior.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="designDoc"></param>
        /// <param name="viewName"></param>
        /// <param name="options"></param>
        /// <param name="objectSerializer">Only needed unless you'd like to override the default behavior of the serializer</param>
        /// <returns></returns>
        public ViewResult<T> View<T>(string designDoc, string viewName, ViewOptions options, IObjectSerializer<T> objectSerializer)
        {
            var uri = databaseBaseUri + "/_design/" + designDoc + "/_view/" + viewName;
            return ProcessGenericResults<T>(uri, options, objectSerializer);                 
        }
        private ViewResult<T> ProcessGenericResults<T>(string uri, ViewOptions options, IObjectSerializer<T> objectSerializer)
        {
            CouchRequest req = GetRequest(options, uri);
            var resp = req.GetResponse();
            return new ViewResult<T>(resp, req.GetRequest(), objectSerializer);
        }
        private ViewResult ProcessResults(string uri, ViewOptions options)
        {
            CouchRequest req = GetRequest(options, uri);
            var resp = req.GetResponse();
            return new ViewResult(resp, req.GetRequest());
        }
        
        private CouchRequest GetRequest(ViewOptions options, string uri)
        {
            if (options != null)
                uri += "?" + options.ToString();
            return GetRequest(uri, options == null ? null : options.Etag).Get().Json();
        }


        /// <summary>
        /// Gets all the documents in the database using the _all_docs uri
        /// </summary>
        /// <returns></returns>
        public ViewResult GetAllDocuments()
        {
            var uri = databaseBaseUri + "/_all_docs";
            return ProcessResults(uri, null);
        }
        public ViewResult GetAllDocuments(ViewOptions options)
        {
            var uri = databaseBaseUri + "/_all_docs";
            return ProcessResults(uri, options);
        }
    }
}