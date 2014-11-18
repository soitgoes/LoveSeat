using System;
using System.Collections.Specialized;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.PeerToPeer;
using System.Threading.Tasks;
using System.Web;
using LoveSeat.Interfaces;
using LoveSeat.Support;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
    public class CouchDatabase : CouchBase, IDocumentDatabase
    {
        #region Fields and Properties
        private readonly string databaseName;
        private IObjectSerializer objectSerializer;
        public IObjectSerializer ObjectSerializer
        {
            get { return objectSerializer ?? (objectSerializer = new DefaultSerializer()); }
            set { objectSerializer = value; }
        }

        public Uri DatabaseBaseUri
        {
            get { return databaseBaseUri; }
        }

        private readonly Uri databaseBaseUri;
        private string defaultDesignDoc = null;

        #endregion

        public CouchDatabase(ICouchConnection couchConnection)
            : base(couchConnection)
        {
            this.databaseBaseUri = couchConnection.BaseUri.Combine(couchConnection.DatabaseName);

        }      

        /// <summary>
        /// Creates a document using the json provided. 
        /// No validation or smarts attempted here by design for simplicities sake
        /// </summary>
        /// <param name="id">Id of Document</param>
        /// <param name="jsonForDocument"></param>
        /// <returns></returns>
        public CouchResponseObject CreateDocument(string id, string jsonForDocument)
        {
            var jobj = JObject.Parse(jsonForDocument);
            if (jobj.Value<object>("_rev") == null)
                jobj.Remove("_rev");
            var resp = GetRequest(databaseBaseUri.Combine("/" + id))
                .Put().Form()
                .Data(jobj.ToString(Formatting.None))
                .GetCouchResponse();
            return
                resp.GetJObject();
        }

        public CouchResponseObject CreateDocument(IBaseObject doc)
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
        public CouchResponseObject CreateDocument(string jsonForDocument)
        {
            var json = JObject.Parse(jsonForDocument); //to make sure it's valid json
            var jobj =
                GetRequest(databaseBaseUri.Combine("/")).Post().Json().Data(jsonForDocument).GetCouchResponse().GetJObject();
            return jobj;
        }
        public CouchResponseObject DeleteDocument(string id, string rev)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(rev))
                throw new Exception("Both id and rev must have a value that is not empty");
            return GetRequest(databaseBaseUri.Combine("/" + id + "?rev=" + rev)).Delete().Form().GetCouchResponse().GetJObject();
        }

        /// <summary>
        ///     Returns null if document is not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual T GetDocument<T>(string id, bool attachments, IObjectSerializer objectSerializer)
        {
            ICouchResponse resp =
                GetRequest(databaseBaseUri.Combine(String.Format("{0}{1}", id,
                    attachments ? "?attachments=true" : string.Empty))).Get().Json().GetCouchResponse();
            if (resp.StatusCode == HttpStatusCode.NotFound) return default(T);

            return objectSerializer.Deserialize<T>(resp.ResponseString);
        }

        public virtual async Task<T> GetDocumentAsync<T>(string id, bool attachments, IObjectSerializer objectSerializer)
        {
            ICouchResponse resp =
                await
                    GetRequest(databaseBaseUri.Combine(String.Format("{0}{1}", id,
                        attachments ? "?attachments=true" : string.Empty))).Get().Json().GetCouchResponseAsync();
            if (resp.StatusCode == HttpStatusCode.NotFound) return default(T);

            return objectSerializer.Deserialize<T>(resp.ResponseString);
        }


        public virtual T GetDocument<T>(string id, IObjectSerializer objectSerializer)
        {
            return GetDocument<T>(id, false, objectSerializer);
        }

        public virtual Task<T> GetDocumentAsync<T>(string id, IObjectSerializer objectSerializer)
        {
            return GetDocumentAsync<T>(id, false, objectSerializer);
        }

        public virtual T GetDocument<T>(string id, bool attachments)
        {
            return GetDocument<T>(id, attachments, ObjectSerializer);
        }

        public virtual Task<T> GetDocumentAsync<T>(string id, bool attachments)
        {
            return GetDocumentAsync<T>(id, attachments, ObjectSerializer);
        }

        public virtual T GetDocument<T>(string id)
        {
            return GetDocument<T>(id, false);
        }

        public virtual Task<T> GetDocumentAsync<T>(string id)
        {
            return GetDocumentAsync<T>(id, false);
        }

        public virtual T GetDocument<T>(Guid id, bool attachments, IObjectSerializer objectSerializer)
        {
            return GetDocument<T>(id.ToString(), attachments, objectSerializer);
        }

        public virtual T GetDocument<T>(Guid id, IObjectSerializer objectSerializer)
        {
            return GetDocument<T>(id, false, objectSerializer);
        }

        public virtual T GetDocument<T>(Guid id, bool attachments)
        {
            return GetDocument<T>(id.ToString(), attachments);
        }

        public virtual Task<T> GetDocumentAsync<T>(Guid id, bool attachments)
        {
            return GetDocumentAsync<T>(id.ToString(), attachments);
        }

        public virtual T GetDocument<T>(Guid id)
        {
            return GetDocument<T>(id, false);
        }

        public virtual Task<T> GetDocumentAsync<T>(Guid id)
        {
            return GetDocumentAsync<T>(id, false);
        }


        public virtual Document GetDocument(string id, bool attachments)
        {
            var uri = databaseBaseUri.Combine(String.Format("{0}{1}", id,
                attachments ? "?attachments=true" : string.Empty));
            ICouchResponse resp =
                GetRequest(uri)
                    .Get().Json().GetCouchResponse();
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;
            return resp.GetCouchDocument();
        }
        public virtual async Task<Document> GetDocumentAsync(string id, bool attachments)
        {
            ICouchResponse resp = await
                GetRequest(databaseBaseUri.Combine(String.Format("{0}{1}", id,
                    attachments ? "?attachments=true" : string.Empty)))
                    .Get().Json().GetCouchResponseAsync();
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;
            return resp.GetCouchDocument();
        }


        public virtual Document GetDocument(string id)
        {
            return GetDocument(id, false);
        }

        public virtual Task<Document> GetDocumentAsync(string id)
        {
            return GetDocumentAsync(id, false);
        }

        /// <summary>
        /// Request multiple documents 
        /// in a single request.
        /// </summary>
        /// <param name="keyLst"></param>
        /// <returns></returns>
        public virtual ViewResult GetDocuments(Keys keyLst)
        {
            // serialize list of keys to json
            string data = Newtonsoft.Json.JsonConvert.SerializeObject(keyLst);
            var viewOptions = new ViewOptions
            {
                IncludeDocs = true,
                Keys = keyLst.Values.Select(x => new KeyOptions(x)).ToArray()
            };

            var resp = GetRequest(viewOptions, databaseBaseUri.Combine("/_all_docs")).GetCouchResponse();

            if (resp == null) return null;

            if (resp.StatusCode == HttpStatusCode.NotFound) return null;

            var vw = new ViewResult(resp, null);

            return vw;
        }

        /// <summary>
        /// Using the bulk API for the loading of documents.
        /// </summary>
        /// <param name="docs"></param>
        /// <remarks>Here we assume you have either added the correct rev, id, or _deleted attribute to each document.  The response will indicate if there were any errors.
        /// Please note that the max_document_size configuration variable in CouchDB limits the maximum request size to CouchDB.</remarks>
        /// <returns>JSON of updated documents in the BulkDocumentResponse class.  </returns>
        public virtual BulkDocumentResponses SaveDocuments(Documents docs, bool all_or_nothing)
        {
            var query = "";
            if (all_or_nothing == true)
            {
                query = "?all_or_nothing=true";
            }

            var uri = databaseBaseUri.Combine("/_bulk_docs" + query);

            string data = Newtonsoft.Json.JsonConvert.SerializeObject(docs);
             


            var couchRequest = GetRequest(uri).Post().Json().Data(data);
            Logger.DebugFormat("SaveDocuments: {0}", uri);
            var resp = couchRequest.GetCouchResponse();

            if (resp == null)
            {
                throw new System.Exception("Response returned null.");
            }

            if (resp.StatusCode != HttpStatusCode.Created && resp.StatusCode != HttpStatusCode.Accepted)
            {
                throw new System.Exception(string.Format("Response returned with a HTTP status code of {0} - {1} - responseString: {2}", resp.StatusCode , resp.StatusDescription, resp.ResponseString));
            }

            // Get response
            string x = resp.ResponseString;

            // Convert to Bulk response
            var bulk = Newtonsoft.Json.JsonConvert.DeserializeObject<BulkDocumentResponses>(x);

            return bulk;
        }


        /// <summary>
        /// Adds an attachment to a document.  If revision is not specified then the most recent will be fetched and used.  Warning: if you need document update conflicts to occur please use the method that specifies the revision
        /// </summary>
        /// <param name="id">id of the couch Document</param>
        /// <param name="attachment">byte[] of of the attachment.  Use File.ReadAllBytes()</param>
        /// <param name="filename">filename of the attachment</param>
        /// <param name="contentType">Content Type must be specifed</param>	
        public CouchResponseObject AddAttachment(string id, byte[] attachment, string filename, string contentType)
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
        public CouchResponseObject AddAttachment(string id, string rev, byte[] attachment, string filename, string contentType)
        {
            return
                GetRequest(databaseBaseUri.Combine(string.Format("{0}/{1}?rev={2}", id, filename, rev))).Put().ContentType(contentType).Data(attachment).GetCouchResponse().GetJObject();
        }
        /// <summary>
        /// Adds an attachment to a document.  If revision is not specified then the most recent will be fetched and used.  Warning: if you need document update conflicts to occur please use the method that specifies the revision
        /// </summary>
        /// <param name="id">id of the couch Document</param>
        /// <param name="attachmentStream">Stream of the attachment.</param>
        /// <param name="contentType">Content Type must be specifed</param>	
        public CouchResponseObject AddAttachment(string id, Stream attachmentStream, string filename, string contentType)
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
        public CouchResponseObject AddAttachment(string id, string rev, Stream attachmentStream, string filename, string contentType)
        {
            return
                GetRequest(databaseBaseUri.Combine(string.Format("{0}/{1}?rev={2}", id, filename, rev))).Put().ContentType(contentType).Data(attachmentStream).GetCouchResponse().GetJObject();
        }

        public Stream GetAttachmentStream(Document doc, string attachmentName)
        {
            return GetAttachmentStream(doc.Id, doc.Rev, attachmentName);
        }
        public Stream GetAttachmentStream(string docId, string rev, string attachmentName)
        {
            return GetRequest(databaseBaseUri.Combine(string.Format("{0}/{1}", docId, HttpUtility.UrlEncode(attachmentName)))).Get().GetHttpResponse().GetResponseStream();
        }
        public Stream GetAttachmentStream(string docId, string attachmentName)
        {
            var doc = GetDocument(docId);
            if (doc == null) return null;
            return GetAttachmentStream(docId, doc.Rev, attachmentName);
        }
        public CouchResponseObject DeleteAttachment(string id, string rev, string attachmentName)
        {
            return GetRequest(databaseBaseUri.Combine(string.Format("{0}/{1}?rev={2}", id, attachmentName, rev))).Json().Delete().GetCouchResponse().GetJObject();
        }
        public CouchResponseObject DeleteAttachment(string id, string attachmentName)
        {
            var doc = GetDocument(id);
            return DeleteAttachment(doc.Id, doc.Rev, attachmentName);
        }

        public CouchResponseObject SaveDocument(Document document)
        {
            if (document.Rev == null)
                return CreateDocument(document);

            var resp = GetRequest(databaseBaseUri.Combine(string.Format("{0}?rev={1}", document.Id, document.Rev))).Put().Form().Data(document).GetCouchResponse();
            return resp.GetJObject();
        }

        /// <summary>
        /// Gets the results of a view with no view parameters.  Use the overload to pass parameters
        /// </summary>
        /// <param name="viewName">The name of the view</param>
        /// <param name="designDoc">The design doc on which the view resides</param>
        /// <returns></returns>
        public virtual ViewResult<T> View<T>(string viewName, string designDoc)
        {
            return View<T>(viewName, null, designDoc);
        }

        /// <summary>
        /// Gets the results of the view using the defaultDesignDoc and no view parameters.  Use the overloads to specify options.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewName"></param>
        /// <returns></returns>
		public virtual ViewResult<T> View<T>(string viewName)
        {
            ThrowDesignDocException();
            return View<T>(viewName, defaultDesignDoc);
        }
        public virtual ViewResult View(string viewName)
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
            return CheckAccepted(GetRequest(new Uri(databaseBaseUri + "/_view_cleanup")).Post().Json().GetCouchResponse());
        }

        /// <summary>
        /// Compact the current database
        /// </summary>
        /// <returns></returns>
        public JObject Compact()
        {
            return CheckAccepted(GetRequest(new Uri(databaseBaseUri + "/_compact")).Post().Json().GetCouchResponse());
        }

        /// <summary>
        /// Compact a view.
        /// </summary>
        /// <param name="designDoc">The view to compact</param>
        /// <returns></returns>
        /// <remarks>Requires admin permissions.</remarks>
        public JObject Compact(string designDoc)
        {
            return CheckAccepted(GetRequest(databaseBaseUri.Combine("/_compact/" + designDoc)).Post().Json().GetCouchResponse());
        }

        private static JObject CheckAccepted(ICouchResponse resp)
        {
            if (resp == null)
            {
                throw new System.Exception("Response returned null.");
            }

            if (resp.StatusCode != HttpStatusCode.Accepted)
            {
                throw new System.Exception(string.Format("Response return with a HTTP Code of {0} - {1}", resp.StatusCode, resp.StatusDescription));
            }

            return resp.GetJObject();

        }


        public string Show(string showName, string docId)
        {
            ThrowDesignDocException();
            return Show(showName, docId, defaultDesignDoc);
        }

        private void ThrowDesignDocException()
        {
            if (string.IsNullOrEmpty(defaultDesignDoc))
                throw new Exception("You must use SetDefaultDesignDoc prior to using this signature.  Otherwise explicitly specify the design doc in the other overloads.");
        }

        public string Show(string showName, string docId, string designDoc)
        {
            //TODO:  add in Etag support for Shows
            var uri = databaseBaseUri.Combine(string.Format("_design/{0}/_show/{1}/{2}", designDoc, showName, docId));
            var req = GetRequest(uri);
            return req.GetCouchResponse().ResponseString;
        }
        public IListResult List(string listName, string viewName, ViewOptions options, string designDoc)
        {
            var uri = databaseBaseUri.Combine(string.Format("_design/{0}/_list/{1}/{2}{3}", designDoc, listName, viewName, options.ToString()));
            var req = GetRequest(uri);
            return new ListResult(req.GetRequest(), req.GetCouchResponse());
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

        private ViewResult<T> ProcessGenericResults<T>(Uri uri, ViewOptions options)
        {
            var req = GetRequest(options, uri);
            var resp = req.GetCouchResponse();

            bool includeDocs = false;
            if (options != null)
            {
                includeDocs = options.IncludeDocs ?? false;
            }

            return new ViewResult<T>(resp, req.GetRequest(), ObjectSerializer, includeDocs);
        }
        /// <summary>
        /// Gets the results of the view using any and all parameters
        /// </summary>
        /// <param name="viewName">The name of the view</param>
        /// <param name="options">Options such as startkey etc.</param>
        /// <param name="designDoc">The design doc on which the view resides</param>
        /// <returns></returns>
		public virtual ViewResult<T> View<T>(string viewName, ViewOptions options, string designDoc)
        {
            var uri = databaseBaseUri.Combine(string.Format("_design/{0}/_view/{1}", designDoc, viewName));
            return ProcessGenericResults<T>(uri, options);
        }
        /// <summary>
        /// Allows you to specify options and uses the defaultDesignDoc Specified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewName"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public ViewResult<T> View<T>(string viewName, ViewOptions options)
        {
            ThrowDesignDocException();
            return View<T>(viewName, options, defaultDesignDoc);
        }

		public virtual ViewResult View(string viewName, ViewOptions options, string designDoc)
        {
            var uri = databaseBaseUri.Combine(string.Format("_design/{0}/_view/{1}", designDoc, viewName));
            return ProcessResults(uri, options);
        }

		public virtual ViewResult View(string viewName, ViewOptions options)
        {
            ThrowDesignDocException();
            return View(viewName, options, this.defaultDesignDoc);
        }
        private ViewResult ProcessResults(Uri uri, ViewOptions options)
        {
            var req = GetRequest(options, uri);
            var resp = req.GetCouchResponse();
            return new ViewResult(resp, req.GetRequest());
        }

        protected ICouchRequest GetRequest(ViewOptions options, Uri uri)
        {
            if (options != null)
                uri = uri.Combine(options.ToString());
            var request = GetRequest(uri, options == null ? null : options.Etag).Get().Json();
            if (options != null && options.isAtKeysSizeLimit)
            {
                // Encode the keys parameter in the request body and turn it into a POST request.
                string keys = "{\"keys\": [" + String.Join(",", options.Keys.Select(k => k.ToRawString()).ToArray()) + "]}";
                request.Post().Data(keys);
            }
            return request;
        }


        /// <summary>
        /// Gets all the documents in the database using the _all_docs uri
        /// </summary>
        /// <returns></returns>
        public ViewResult GetAllDocuments()
        {
            var uri = databaseBaseUri.Combine("/_all_docs");
            return ProcessResults(uri, null);
        }
        public ViewResult GetAllDocuments(ViewOptions options)
        {
            var uri = databaseBaseUri.Combine("/_all_docs");
            return ProcessResults(uri, options);
        }

		#region Exist
		public bool DoesDbExist()
		{
            var uri = databaseBaseUri.Combine("/_all_docs");
			var request = GetRequest(uri);
			request.GetRequest().Method = "HEAD";

			var resp = request.GetCouchResponse();

			return resp.StatusCode == HttpStatusCode.OK;
		}
		#endregion

        #region Security
        public object GetSecurityDocument()
        {
            var uri = databaseBaseUri.Combine("/_security");

            var docResult = GetRequest(uri).Get().Json().GetCouchResponse().GetJObject();

            var sDoc = Newtonsoft.Json.JsonConvert.DeserializeObject<SecurityDocument>(docResult.ToString());

            return sDoc;
        }

        /// <summary>
        /// Updates security configuration for the database
        /// </summary>
        /// <param name="sDoc"></param>
        public void UpdateSecurityDocument(object sDoc)
        {
            var uri = databaseBaseUri.Combine("/_security");

            // serialize SecurityDocument to json
            string data = Newtonsoft.Json.JsonConvert.SerializeObject(sDoc);

            var result = GetRequest(uri).Put().Json().Data(data).GetCouchResponse();

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
            writers = new UserType();
        }

    	public NobodyUserType cloudant;
        public UserType admins;
        public UserType readers;
        public UserType writers;

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

	public class NobodyUserType
	{
		public NobodyUserType()
		{
			nobody = new List<string>();
		}

		public List<string> nobody { get; set; }
	}
    #endregion

}