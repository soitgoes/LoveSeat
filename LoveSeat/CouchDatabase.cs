using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using LoveSeat.Interfaces;
using LoveSeat.Support;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
    public class CouchDatabase : CouchBase, IDocumentDatabase
    {
        public IObjectSerializer ObjectSerializer = new DefaultSerializer();

        private readonly string databaseBaseUri;
        private string defaultDesignDoc = null;
        internal CouchDatabase(string baseUri, string databaseName, string username, string password, AuthenticationType aT)
            : base(username, password, aT)
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
        public CouchResponse CreateDocument(string id, string jsonForDocument)
        {
            var jobj = JObject.Parse(jsonForDocument);
            if (jobj.Value<object>("_rev") == null)
                jobj.Remove("_rev");
            var resp = GetRequest(databaseBaseUri + "/" + id)
                .Put().Form()
                .Data(jobj.ToString(Formatting.None))
                .GetResponse();
            return 
                resp.GetJObject();
        }

        public CouchResponse CreateDocument(IBaseObject doc) 
        {
            var serialized = ObjectSerializer.Serialize(doc);
            if (doc.Id != null)
            {
                return CreateDocument(doc.Id, serialized);
            }
            return CreateDocument(serialized);
        }
        /// <summary>
        /// Creates a document when you intend for Couch to generate the id for you.
        /// </summary>
        /// <param name="jsonForDocument">Json for creating the document</param>
        /// <returns>The response as a JObject</returns>
        public CouchResponse CreateDocument(string jsonForDocument)
        {
            var json = JObject.Parse(jsonForDocument); //to make sure it's valid json
            var jobj = 
                GetRequest(databaseBaseUri + "/").Post().Json().Data(jsonForDocument).GetResponse().GetJObject();
            return jobj;
        }        
        public CouchResponse DeleteDocument(string id, string rev)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(rev))
                throw new Exception("Both id and rev must have a value that is not empty");
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
        public T GetDocument<T>(Guid id , IObjectSerializer objectSerializer)
        {
            return GetDocument<T>(id.ToString(), objectSerializer);
        }
        public T GetDocument<T>(Guid id)
        {
            return GetDocument<T>(id.ToString());
        }
        public T GetDocument<T>(string id)
        {
            return GetDocument<T>(id, ObjectSerializer);
        }
        public T GetDocument<T>(string id, IObjectSerializer objectSerializer)
        {
            var resp = GetRequest(databaseBaseUri + "/" + id).Get().Json().GetResponse();
            if (resp.StatusCode == HttpStatusCode.NotFound) return default(T);
            return objectSerializer.Deserialize<T>(resp.GetResponseString());
        }

        /// <summary>
        /// Request multiple documents 
        /// in a single request.
        /// </summary>
        /// <param name="keyLst"></param>
        /// <returns></returns>
        public ViewResult GetDocuments(Keys keyLst)
        {
            // serialize list of keys to json
            string data = Newtonsoft.Json.JsonConvert.SerializeObject(keyLst);
            
            var resp = GetRequest(databaseBaseUri + "/_all_docs").Post().Json().Data(data).GetResponse();

            if (resp == null) return null;

            if (resp.StatusCode == HttpStatusCode.NotFound) return null;

            ViewResult vw = new ViewResult(resp, null);

            return vw;
        }
 
        /// <summary>
        /// Using the bulk API for the loading of documents.
        /// </summary>
        /// <param name="docs"></param>
        /// <remarks>Here we assume you have either added the correct rev, id, or _deleted attribute to each document.  The response will indicate if there were any errors.
        /// Please note that the max_document_size configuration variable in CouchDB limits the maximum request size to CouchDB.</remarks>
        /// <returns>JSON of updated documents in the BulkDocumentResponse class.  </returns>
        public BulkDocumentResponses SaveDocuments(Documents docs, bool all_or_nothing)
        {
            string uri = databaseBaseUri + "/_bulk_docs";

            string data = Newtonsoft.Json.JsonConvert.SerializeObject(docs);

            if (all_or_nothing == true)
            {
                uri = uri + "?all_or_nothing=true";
            }

            HttpWebResponse resp = GetRequest(uri).Post().Json().Data(data).GetResponse();

            if (resp == null)
            {
                throw new System.Exception("Response returned null.");
            }

            if (resp.StatusCode != HttpStatusCode.Created)
            {
                throw new System.Exception("Response returned with a HTTP status code of " + resp.StatusCode + " - " + resp.StatusDescription);    
            }

            // Get response
            string x = resp.GetResponseString();
                        
            // Convert to Bulk response
            BulkDocumentResponses bulk = Newtonsoft.Json.JsonConvert.DeserializeObject<BulkDocumentResponses>(x);

            return bulk;
        }

        
        /// <summary>
        /// Adds an attachment to a document.  If revision is not specified then the most recent will be fetched and used.  Warning: if you need document update conflicts to occur please use the method that specifies the revision
        /// </summary>
        /// <param name="id">id of the couch Document</param>
        /// <param name="attachment">byte[] of of the attachment.  Use File.ReadAllBytes()</param>
        /// <param name="filename">filename of the attachment</param>
        /// <param name="contentType">Content Type must be specifed</param>	
        public CouchResponse AddAttachment(string id, byte[] attachment, string filename, string contentType)
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
        public CouchResponse AddAttachment(string id, string rev, byte[] attachment, string filename, string contentType)
        {
            return
                GetRequest(databaseBaseUri + "/" + id + "/" + filename + "?rev=" + rev).Put().ContentType(contentType).Data(attachment).GetResponse().GetJObject();
        }
        /// <summary>
        /// Adds an attachment to a document.  If revision is not specified then the most recent will be fetched and used.  Warning: if you need document update conflicts to occur please use the method that specifies the revision
        /// </summary>
        /// <param name="id">id of the couch Document</param>
        /// <param name="attachmentStream">Stream of the attachment.</param>
        /// <param name="contentType">Content Type must be specifed</param>	
        public CouchResponse AddAttachment(string id, Stream attachmentStream, string filename, string contentType)
        {
            var doc = GetDocument(id);
            return AddAttachment(id, doc.Rev, attachmentStream, filename, contentType);
        }
        /// <summary>
        /// Adds an attachment to the documnet.  Rev must be specified on this signature.  If you want to attach no matter what then use the method without the rev param
        /// </summary>
        /// <param name="id">id of the couch Document</param>
        /// <param name="rev">revision _rev of the Couch Document</param>
        /// <param name="attachmentStream">Stream of of the attachment.  Use File.ReadAllBytes()</param>
        /// <param name="filename">filename of the attachment</param>
        /// <param name="contentType">Content Type must be specifed</param>			
        /// <returns></returns>
        public CouchResponse AddAttachment(string id, string rev, Stream attachmentStream, string filename, string contentType)
        {
            return
                GetRequest(databaseBaseUri + "/" + id + "/" + filename + "?rev=" + rev).Put().ContentType(contentType).Data(attachmentStream).GetResponse().GetJObject();
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
        public CouchResponse DeleteAttachment(string id, string rev, string attachmentName)
        {
            return GetRequest(databaseBaseUri + "/" + id + "/" + attachmentName + "?rev=" + rev).Json().Delete().GetResponse().GetJObject();
        }
        public CouchResponse DeleteAttachment(string id, string attachmentName)
        {
            var doc = GetDocument(id);
            return DeleteAttachment(doc.Id, doc.Rev, attachmentName);
        }

        public CouchResponse SaveDocument(Document document)
        {
            if (document.Rev == null)
                return CreateDocument(document);
                    
            var resp = GetRequest(databaseBaseUri + "/" + document.Id + "?rev=" + document.Rev).Put().Form().Data(document).GetResponse();
            return resp.GetJObject();
        }

        /// <summary>
        /// Gets the results of a view with no view parameters.  Use the overload to pass parameters
        /// </summary>
        /// <param name="viewName">The name of the view</param>
        /// <param name="designDoc">The design doc on which the view resides</param>
        /// <returns></returns>
        public ViewResult<T> View<T>(string viewName, string designDoc)
        {
            return View<T>(viewName, null, designDoc);
        }

        /// <summary>
        /// Gets the results of the view using the defaultDesignDoc and no view parameters.  Use the overloads to specify options.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public ViewResult<T> View<T>(string viewName)
        {
            ThrowDesignDocException();
            return View<T>(viewName, defaultDesignDoc);
        }
        public ViewResult View(string viewName)
        {
            ThrowDesignDocException();
            return View(viewName, new ViewOptions());
        }

        /// <summary>
        /// Call view cleanup for a database
        /// </summary>
        /// <returns>JSON success statement if the response code is Accepted</returns>
        public JObject ViewCleanup()
        {
            return CheckAccepted(GetRequest(databaseBaseUri + "/_view_cleanup").Post().Json().GetResponse());
        }

        /// <summary>
        /// Compact the current database
        /// </summary>
        /// <returns></returns>
        public JObject Compact()
        {
            return CheckAccepted(GetRequest(databaseBaseUri + "/_compact").Post().Json().GetResponse());
        }

        /// <summary>
        /// Compact a view.
        /// </summary>
        /// <param name="designDoc">The view to compact</param>
        /// <returns></returns>
        /// <remarks>Requires admin permissions.</remarks>
        public JObject Compact(string designDoc)
        {
            return CheckAccepted(GetRequest(databaseBaseUri + "/_compact/" + designDoc).Post().Json().GetResponse());
        }

        private static JObject CheckAccepted(HttpWebResponse resp)
        {
            if (resp == null) {
                throw new System.Exception("Response returned null.");
            }

            if (resp.StatusCode != HttpStatusCode.Accepted) {
                throw new System.Exception("Response return with a HTTP Code of " + resp.StatusCode + " - " + resp.StatusDescription);
            }

            return resp.GetJObject();

        }


        public string Show (string showName, string docId)
        {
            ThrowDesignDocException();
            return Show(showName, docId,  defaultDesignDoc);
        }

        private void ThrowDesignDocException()
        {
            if (string.IsNullOrEmpty(defaultDesignDoc))
                throw new Exception("You must use SetDefaultDesignDoc prior to using this signature.  Otherwise explicitly specify the design doc in the other overloads.");
        }

        public string Show(string showName, string docId, string designDoc)
        {
            //TODO:  add in Etag support for Shows
            var uri = databaseBaseUri + "/_design/" + designDoc + "/_show/" + showName + "/" + docId;
            var req = GetRequest(uri);
            return req.GetResponse().GetResponseString();
        }
        public IListResult List(string listName, string viewName, ViewOptions options,  string designDoc)
        {            
			var uri = databaseBaseUri + "/_design/" + designDoc + "/_list/" + listName + "/" + viewName + options.ToString();
            var req = GetRequest(uri);
            return new ListResult(req.GetRequest(), req.GetResponse());
        }

        public IListResult List(string listName, string viewName, ViewOptions options)
        {
            ThrowDesignDocException();
            return List(listName, viewName, options, defaultDesignDoc);
        }

        public void SetDefaultDesignDoc(string designDoc)
        {
            this.defaultDesignDoc = designDoc;
        }

        private ViewResult<T> ProcessGenericResults<T>(string uri, ViewOptions options) {
            CouchRequest req = GetRequest(options, uri);
            var resp = req.GetResponse();
            if (resp.StatusCode == HttpStatusCode.BadRequest) {
                throw new CouchException(req.GetRequest(), resp, resp.GetResponseString() + "\n" + req.GetRequest().RequestUri);
            }

            bool includeDocs = options.IncludeDocs ?? false;
            return new ViewResult<T>(resp, req.GetRequest(), ObjectSerializer, includeDocs);
        }
        /// <summary>
        /// Gets the results of the view using any and all parameters
        /// </summary>
        /// <param name="viewName">The name of the view</param>
        /// <param name="options">Options such as startkey etc.</param>
        /// <param name="designDoc">The design doc on which the view resides</param>
        /// <returns></returns>
        public ViewResult<T> View<T>(string viewName, ViewOptions options, string designDoc)
        {
            var uri = databaseBaseUri + "/_design/" + designDoc + "/_view/" + viewName;
            return ProcessGenericResults<T>(uri, options);
        }
        /// <summary>
        /// Allows you to specify options and uses the defaultDesignDoc Specified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewName"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public ViewResult<T>  View<T>(string viewName, ViewOptions options)
        {
            ThrowDesignDocException();
             return View<T>(viewName, options, defaultDesignDoc);
        }
        
        public ViewResult View(string viewName, ViewOptions options, string designDoc)
        {
            var uri = databaseBaseUri + "/_design/" + designDoc + "/_view/" + viewName;
            return ProcessResults(uri, options);
        }

        public ViewResult View(string viewName, ViewOptions options)
        {
            ThrowDesignDocException();
            var uri = databaseBaseUri + "/_design/" + this.defaultDesignDoc + "/_view/" + viewName;
            return ProcessResults(uri, options);
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
                uri +=  options.ToString();
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




        #region Security
        public SecurityDocument getSecurityConfiguration()
        {
            string request = databaseBaseUri + "/_security";

            var docResult = GetRequest(request).Get().Json().GetResponse().GetJObject();

            SecurityDocument sDoc = Newtonsoft.Json.JsonConvert.DeserializeObject<SecurityDocument>(docResult.ToString());

            return sDoc;
        }

        /// <summary>
        /// Updates security configuration for the database
        /// </summary>
        /// <param name="sDoc"></param>
        public void UpdateSecurityDocument(SecurityDocument sDoc)
        {
            string request = databaseBaseUri + "/_security";

            // serialize SecurityDocument to json
            string data = Newtonsoft.Json.JsonConvert.SerializeObject(sDoc);

            var result = GetRequest(request).Put().Json().Data(data).GetResponse();

            if (result.StatusCode != HttpStatusCode.OK) //Check if okay
            {
                throw new WebException("An error occurred while trying to update the security document. StatusDescription: " + result.StatusDescription);
            }
        }

        #endregion
    }

    #region Security Configuration

    // Example: {"admins":{},"readers":{"names":["dave"],"roles":[]}}
    /// <summary>
    /// Security configuration for the database
    /// </summary>
    public class SecurityDocument
    {
        public SecurityDocument()
        {
            admins = new UserType();
            readers = new UserType();
        }


        public UserType admins;
        public UserType readers;
    }

    public class UserType
    {
        public UserType()
        {
            names = new List<string>();
            roles = new List<string>();
        }

        public List<string> names { get; set; }
        public List<string> roles { get; set; }
    }
    #endregion

}