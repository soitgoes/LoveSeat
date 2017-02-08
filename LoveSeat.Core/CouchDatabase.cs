using System;
using System.Linq;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LoveSeat.Core.Interfaces;
using LoveSeat.Core.Support;
using System.Threading.Tasks;

namespace LoveSeat.Core
{
    public class CouchDatabase : CouchBase, IDocumentDatabase
    {
        private readonly string _databaseBaseUri;
        private string _defaultDesignDoc = null;
        public IObjectSerializer _objectSerializer = new DefaultSerializer();

        internal CouchDatabase(string baseUri, string databaseName, string username, string password, AuthenticationType authType)
            : base(username, password, authType)
        {
            _baseUri = baseUri;
            _databaseBaseUri = baseUri + databaseName;
        }

        /// <summary>
        /// Full uri including database
        /// </summary>
        /// <param name="uri"></param>
        public CouchDatabase(Uri uri)
        {
            _baseUri = uri.AbsoluteUri.Replace(uri.AbsolutePath, "");
            _databaseBaseUri = uri.AbsoluteUri;
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
            return CreateDocumentAsync(id, jsonForDocument).GetAwaiter().GetResult();
        }

        public CouchResponseObject CreateDocument(Document doc)
        {
            return CreateDocumentAsync(doc).GetAwaiter().GetResult();
        }

        public CouchResponseObject CreateDocument<T>(Document<T> doc)
            where T : class, IBaseObject
        {
            return CreateDocumentAsync(doc).GetAwaiter().GetResult();
        }

        public CouchResponseObject CreateDocument<T>(T item)
            where T : class, IBaseObject
        {
            return CreateDocumentAsync(item).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a document when you intend for Couch to generate the id for you.
        /// </summary>
        /// <param name="jsonForDocument">Json for creating the document</param>
        /// <returns>The response as a JObject</returns>
        public CouchResponseObject CreateDocument(string jsonForDocument)
        {
            return CreateDocumentAsync(jsonForDocument).GetAwaiter().GetResult();
        }

        public CouchResponseObject DeleteDocument(string id, string rev)
        {
            return DeleteDocumentAsync(id, rev).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Returns null if document is not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Document<T> GetDocument<T>(string id, bool attachments, IObjectSerializer objectSerializer)
            where T : class
        {
            var result = GetDocumentAsync<T>(id, attachments, objectSerializer).GetAwaiter().GetResult();
            return result;
        }

        public Document<T> GetDocument<T>(string id, bool attachments) where T : class
        {
            return GetDocumentAsync<T>(id, attachments, _objectSerializer).GetAwaiter().GetResult();
        }

        public Document<T> GetDocument<T>(string id) 
            where T : class
        {
            return GetDocumentAsync<T>(id, false).GetAwaiter().GetResult();
        }

        public Document<T> GetDocument<T>(Guid id, bool attachments, IObjectSerializer objectSerializer) where T : class
        {
            return GetDocument<T>(id.ToString(), attachments, objectSerializer);
        }

        public Document<T> GetDocument<T>(Guid id, IObjectSerializer objectSerializer) where T : class
        {
            return GetDocument<T>(id, false, objectSerializer);
        }

        public Document<T> GetDocument<T>(Guid id, bool attachments) where T : class
        {
            return GetDocument<T>(id.ToString(), attachments);
        }

        public Document<T> GetDocument<T>(Guid id) where T : class
        {
            return GetDocument<T>(id, false);
        }

        public Document GetDocument(string id, bool attachments)
        {
            return GetDocumentAsync(id, attachments).GetAwaiter().GetResult();
        }

        public Document GetDocument(string id)
        {
            return GetDocument(id, false);
        }

        /// <summary>
        /// Request multiple documents 
        /// in a single request.
        /// </summary>
        /// <param name="keyLst"></param>
        /// <returns></returns>
        public ViewResult GetDocuments(Keys keyLst)
        {
            return GetDocumentsAsync(keyLst).GetAwaiter().GetResult();
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
            return SaveDocumentsAsync(docs, all_or_nothing).GetAwaiter().GetResult();
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
            return AddAttachmentAsync(id, doc.Rev, attachment, filename, contentType).GetAwaiter().GetResult();
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
            return AddAttachmentAsync(id, rev, attachment, filename, contentType).GetAwaiter().GetResult();
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
            return AddAttachmentAsync(id, doc.Rev, attachmentStream, filename, contentType).GetAwaiter().GetResult();
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
            return AddAttachmentAsync(id, rev, attachmentStream, filename, contentType).GetAwaiter().GetResult();
        }

        public Stream GetAttachmentStream(Document doc, string attachmentName)
        {
            return GetAttachmentStreamAsync(doc.Id, doc.Rev, attachmentName).GetAwaiter().GetResult();
        }

        public Stream GetAttachmentStream(string docId, string rev, string attachmentName)
        {
            return GetAttachmentStreamAsync(docId, rev, attachmentName).GetAwaiter().GetResult();
        }

        public Stream GetAttachmentStream(string docId, string attachmentName)
        {
            var doc = GetDocument(docId);
            if (doc == null)
                return null;

            return GetAttachmentStreamAsync(docId, doc.Rev, attachmentName).GetAwaiter().GetResult();
        }

        public CouchResponseObject DeleteAttachment(string id, string rev, string attachmentName)
        {
            return DeleteAttachmentAsync(id, rev, attachmentName).GetAwaiter().GetResult();
        }

        public CouchResponseObject DeleteAttachment(string id, string attachmentName)
        {
            var doc = GetDocument(id);
            return DeleteAttachmentAsync(doc.Id, doc.Rev, attachmentName).GetAwaiter().GetResult();
        }

        public CouchResponseObject SaveDocument(Document document)
        {
            if (document == null)
                throw new Exception("Cannot pass null to CouchDatabase.SaveDocument");

            if (document.Rev == null)
                return CreateDocumentAsync(document).GetAwaiter().GetResult();

            var response = SaveDocumentAsync(document).GetAwaiter().GetResult();
            return response;
        }

        /// <summary>
        /// Gets the results of a view with no view parameters.  Use the overload to pass parameters
        /// </summary>
        /// <param name="viewName">The name of the view</param>
        /// <param name="designDoc">The design doc on which the view resides</param>
        /// <returns></returns>
        public ViewResult<T> View<T>(string viewName, string designDoc) where T : class
        {
            return View<T>(viewName, null, designDoc);
        }

        /// <summary>
        /// Gets the results of the view using the defaultDesignDoc and no view parameters.  Use the overloads to specify options.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public ViewResult<T> View<T>(string viewName) where T : class
        {
            ThrowDesignDocException();
            return View<T>(viewName, _defaultDesignDoc);
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
            return ViewCleanupAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Compact the current database
        /// </summary>
        /// <returns></returns>
        public JObject Compact()
        {
            return CompactAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Compact a view.
        /// </summary>
        /// <param name="designDoc">The view to compact</param>
        /// <returns></returns>
        /// <remarks>Requires admin permissions.</remarks>
        public JObject Compact(string designDoc)
        {
            return CompactAsync(designDoc).GetAwaiter().GetResult();
        }

        private static JObject CheckAccepted(CouchResponse resp)
        {
            if (resp == null)
                throw new Exception("Response returned null.");

            if (resp.StatusCode != HttpStatusCode.Accepted)
                throw new Exception(string.Format("Response return with a HTTP Code of {0} - {1}", resp.StatusCode, resp.StatusDescription));

            return resp.GetJObject();
        }

        public string Show(string showName, string docId)
        {
            ThrowDesignDocException();
            return Show(showName, docId, _defaultDesignDoc);
        }

        private void ThrowDesignDocException()
        {
            if (string.IsNullOrEmpty(_defaultDesignDoc))
                throw new Exception("You must use SetDefaultDesignDoc prior to using this signature.  Otherwise explicitly specify the design doc in the other overloads.");
        }

        public string Show(string showName, string docId, string designDoc)
        {
            return ShowAsync(showName, docId, designDoc).GetAwaiter().GetResult();
        }

        public IListResult List(string listName, string viewName, ViewOptions options, string designDoc)
        {
            return ListAsync(listName, viewName, options, designDoc).GetAwaiter().GetResult();
        }

        public IListResult List(string listName, string viewName, ViewOptions options)
        {
            ThrowDesignDocException();
            return List(listName, viewName, options, _defaultDesignDoc);
        }

        public void SetDefaultDesignDoc(string designDoc)
        {
            _defaultDesignDoc = designDoc;
        }

        private ViewResult<T> ProcessGenericResults<T>(string uri, ViewOptions options) 
            where T : class
        {
            return ProcessGenericResultsAsync<T>(uri, options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets the results of the view using any and all parameters
        /// </summary>
        /// <param name="viewName">The name of the view</param>
        /// <param name="options">Options such as startkey etc.</param>
        /// <param name="designDoc">The design doc on which the view resides</param>
        /// <returns></returns>
        public ViewResult<T> View<T>(string viewName, ViewOptions options, string designDoc) 
            where T : class
        {
            return ViewAsync<T>(viewName, options, designDoc).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Allows you to specify options and uses the defaultDesignDoc Specified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewName"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public ViewResult<T> View<T>(string viewName, ViewOptions options) where T : class
        {
            ThrowDesignDocException();
            return View<T>(viewName, options, _defaultDesignDoc);
        }

        public ViewResult View(string viewName, ViewOptions options, string designDoc)
        {
            var uri = string.Format("{0}/_design/{1}/_view/{2}", _databaseBaseUri, designDoc, viewName);
            return ProcessResults(uri, options);
        }

        public ViewResult View(string viewName, ViewOptions options)
        {
            ThrowDesignDocException();
            return View(viewName, options, _defaultDesignDoc);
        }

        private ViewResult ProcessResults(string uri, ViewOptions options)
        {
            return ProcessResultsAsync(uri, options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets all the documents in the database using the _all_docs uri
        /// </summary>
        /// <returns></returns>
        public ViewResult GetAllDocuments()
        {
            return GetAllDocumentsAsync().GetAwaiter().GetResult();
        }

        public ViewResult GetAllDocuments(ViewOptions options)
        {
            return GetAllDocumentsAsync(options).GetAwaiter().GetResult();
        }

        public SecurityDocument GetSecurityConfiguration()
        {
            string request = _databaseBaseUri + "/_security";

            var docResult = GetRequest(request).Get().Json().GetCouchResponse().GetJObject();

            SecurityDocument securityDoc = JsonConvert.DeserializeObject<SecurityDocument>(docResult.ToString());
            return securityDoc;
        }

        /// <summary>
        /// Updates security configuration for the database
        /// </summary>
        /// <param name="securityDoc"></param>
        public void UpdateSecurityDocument(SecurityDocument securityDoc)
        {
            UpdateSecurityDocumentAsync(securityDoc).GetAwaiter().GetResult();
        }

        public async Task<CouchResponseObject> CreateDocumentAsync(string id, string jsonForDocument)
        {
            var jobj = JObject.Parse(jsonForDocument);
            if (jobj.Value<object>("_rev") == null)
                jobj.Remove("_rev");

            var resp = await (await (await GetRequestAsync(_databaseBaseUri + "/" + id).ConfigureAwait(false))
                .Put()
                .Json()
                .DataAsync(jobj.ToString(Formatting.None)).ConfigureAwait(false))
                .GetCouchResponseAsync().ConfigureAwait(false);

            return resp.GetJObject();
        }

        public async Task<CouchResponseObject> CreateDocumentAsync<T>(T item)
             where T : class, IBaseObject
        {
            var serialized = _objectSerializer.Serialize(item);
            if (item.Id != null)
                return await CreateDocumentAsync(item.Id, serialized).ConfigureAwait(false);

            return await CreateDocumentAsync(serialized).ConfigureAwait(false);
        }

        public async Task<CouchResponseObject> CreateDocumentAsync(Document doc)
        {
            var serialized = _objectSerializer.Serialize(doc.JObject);
            if (doc.Id != null)
                return await CreateDocumentAsync(doc.Id, serialized).ConfigureAwait(false);

            return await CreateDocumentAsync(serialized).ConfigureAwait(false);
        }

        public async Task<CouchResponseObject> CreateDocumentAsync<T>(Document<T> doc)
            where T : class, IBaseObject
        {
            var serialized = _objectSerializer.Serialize(doc.JObject);
            if (doc.Id != null)
                return await CreateDocumentAsync(doc.Id, serialized);

            return await CreateDocumentAsync(serialized);
        }

        public async Task<CouchResponseObject> CreateDocumentAsync(string jsonForDocument)
        {
            // To make sure it's valid json
            var json = JObject.Parse(jsonForDocument);
            var jobj = (await (await (await GetRequestAsync(_databaseBaseUri + "/").ConfigureAwait(false))
                .Post()
                .Json()
                .DataAsync(jsonForDocument).ConfigureAwait(false))
                .GetCouchResponseAsync().ConfigureAwait(false))
                .GetJObject();

            return jobj;
        }

        public async Task<CouchResponseObject> DeleteDocumentAsync(string id, string rev)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(rev))
                throw new Exception("Both id and rev must have a value that is not empty");

            return (await (await GetRequestAsync(_databaseBaseUri + "/" + id + "?rev=" + rev).ConfigureAwait(false))
                .Delete()
                .Form()
                .GetCouchResponseAsync().ConfigureAwait(false))
                .GetJObject();
        }

        private async Task<Document<T>> GetDocumentAsync<T>(string id, bool attachments, IObjectSerializer objectSerializer)
            where T : class
        {
            var response = await (await GetRequestAsync(string.Format("{0}/{1}{2}", _databaseBaseUri, id, attachments ? "?attachments=true" : string.Empty)).ConfigureAwait(false))
                .Get()
                .Json()
                .GetCouchResponseAsync().ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            return objectSerializer.DeserializeToDoc<T>(response.ResponseString);
        }

        private async Task<Document> GetDocumentAsync(string id, bool attachments)
        {
            var response = await (await GetRequestAsync(string.Format("{0}/{1}{2}", _databaseBaseUri, id, attachments ? "?attachments=true" : string.Empty)).ConfigureAwait(false))
                .Get()
                .Json()
                .GetCouchResponseAsync().ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            return response.GetCouchDocument();
        }

        public Task<Document<T>> GetDocumentAsync<T>(string id)
            where T : class
        {
            return GetDocumentAsync<T>(id, false);
        }

        public Task<Document<T>> GetDocumentAsync<T>(string id, bool attachments) 
            where T : class
        {
            return GetDocumentAsync<T>(id, attachments, _objectSerializer);
        }

        private async Task<CouchRequest> GetRequestAsync(ViewOptions options, string uri)
        {
            if (options != null)
                uri += options.ToString();

            CouchRequest request = (await GetRequestAsync(uri, options == null ? null : options.Etag).ConfigureAwait(false))
                .Get()
                .Json();

            if (options != null && options.isAtKeysSizeLimit)
            {
                // Encode the keys parameter in the request body and turn it into a POST request.
                string keys = "{\"keys\": [" + string.Join(",", options.Keys.Select(k => k.ToRawString()).ToArray()) + "]}";

                await request
                    .Post()
                    .DataAsync(keys)
                    .ConfigureAwait(false);
            }

            return request;
        }

        /// <summary>
        /// Request multiple documents 
        /// in a single request.
        /// </summary>
        /// <param name="keyLst"></param>
        /// <returns></returns>
        public async Task<ViewResult> GetDocumentsAsync(Keys keyLst)
        {
            // Serialize list of keys to json
            string data = JsonConvert.SerializeObject(keyLst);
            ViewOptions viewOptions = new ViewOptions
            {
                IncludeDocs = true,
                Keys = keyLst.Values.Select(x => new KeyOptions(x)).ToArray()
            };

            CouchResponse response = await (await GetRequestAsync(viewOptions, _databaseBaseUri + "/_all_docs").ConfigureAwait(false))
                .GetCouchResponseAsync().ConfigureAwait(false);

            if (response == null)
                return null;

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            return new ViewResult(response, null);
        }

        /// <summary>
        /// Using the bulk API for the loading of documents.
        /// </summary>
        /// <param name="docs"></param>
        /// <remarks>Here we assume you have either added the correct rev, id, or _deleted attribute to each document.  The response will indicate if there were any errors.
        /// Please note that the max_document_size configuration variable in CouchDB limits the maximum request size to CouchDB.</remarks>
        /// <returns>JSON of updated documents in the BulkDocumentResponse class.  </returns>
        public async Task<BulkDocumentResponses> SaveDocumentsAsync(Documents docs, bool all_or_nothing)
        {
            string uri = _databaseBaseUri + "/_bulk_docs";

            string data = JsonConvert.SerializeObject(docs);

            if (all_or_nothing == true)
                uri = uri + "?all_or_nothing=true";

            CouchResponse response = await (await (await GetRequestAsync(uri).ConfigureAwait(false))
                .Post()
                .Json()
                .DataAsync(data).ConfigureAwait(false))
                .GetCouchResponseAsync().ConfigureAwait(false);

            if (response == null)
                throw new Exception("Response returned null.");

            if (response.StatusCode != HttpStatusCode.Created)
                throw new Exception("Response returned with a HTTP status code of " + response.StatusCode + " - " + response.StatusDescription);

            // Convert to Bulk response
            BulkDocumentResponses bulk = JsonConvert.DeserializeObject<BulkDocumentResponses>(response.ResponseString);
            return bulk;
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
        public async Task<CouchResponseObject> AddAttachmentAsync(string id, string rev, byte[] attachment, string filename, string contentType)
        {
            return (await (await (await GetRequestAsync(string.Format("{0}/{1}/{2}?rev={3}", _databaseBaseUri, id, filename, rev)).ConfigureAwait(false))
                .Put()
                .ContentType(contentType)
                .DataAsync(attachment).ConfigureAwait(false))
                .GetCouchResponseAsync().ConfigureAwait(false))
                .GetJObject();
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
        public async Task<CouchResponseObject> AddAttachmentAsync(string id, string rev, Stream attachmentStream, string filename, string contentType)
        {
            return (await (await (await GetRequestAsync(string.Format("{0}/{1}/{2}?rev={3}", _databaseBaseUri, id, filename, rev)).ConfigureAwait(false))
                .Put()
                .ContentType(contentType)
                .DataAsync(attachmentStream).ConfigureAwait(false))
                .GetCouchResponseAsync().ConfigureAwait(false))
                .GetJObject();
        }

        public async Task<Stream> GetAttachmentStreamAsync(string docId, string rev, string attachmentName)
        {
            var response = (await (await GetRequestAsync(string.Format("{0}/{1}/{2}", _databaseBaseUri, docId, WebUtility.UrlEncode(attachmentName))).ConfigureAwait(false))
                .Get()
                .GetHttpResponseAsync().ConfigureAwait(false))
                .GetResponseStream();

            return response;
        }

        public async Task<CouchResponseObject> DeleteAttachmentAsync(string id, string rev, string attachmentName)
        {
            var response = (await (await GetRequestAsync(string.Format("{0}/{1}/{2}?rev={3}", _databaseBaseUri, id, attachmentName, rev)).ConfigureAwait(false))
                .Json()
                .Delete()
                .GetCouchResponseAsync().ConfigureAwait(false))
                .GetJObject();

            return response;
        }

        public async Task<CouchResponseObject> SaveDocumentAsync(Document document)
        {
            if (document == null)
                throw new Exception("Cannot pass null to CouchDatabase.SaveDocument");

            if (document.Rev == null)
                return await CreateDocumentAsync(document).ConfigureAwait(false);

            var response = await (await (await GetRequestAsync(string.Format("{0}/{1}?rev={2}", _databaseBaseUri, document.Id, document.Rev)).ConfigureAwait(false))
                .Put()
                .Form()
                .DataAsync(document.JObject).ConfigureAwait(false))
                .GetCouchResponseAsync().ConfigureAwait(false);

            return response.GetJObject();
        }

        /// <summary>
        /// Call view cleanup for a database
        /// </summary>
        /// <returns>JSON success statement if the response code is Accepted</returns>
        public async Task<JObject> ViewCleanupAsync()
        {
            var response = await (await GetRequestAsync(_databaseBaseUri + "/_view_cleanup").ConfigureAwait(false))
                .Post()
                .Json()
                .GetCouchResponseAsync().ConfigureAwait(false);

            return CheckAccepted(response);
        }

        /// <summary>
        /// Compact the current database
        /// </summary>
        /// <returns></returns>
        public async Task<JObject> CompactAsync()
        {
            var response = await (await GetRequestAsync(_databaseBaseUri + "/_compact").ConfigureAwait(false))
                .Post()
                .Json()
                .GetCouchResponseAsync().ConfigureAwait(false);

            return CheckAccepted(response);
        }

        /// <summary>
        /// Compact a view.
        /// </summary>
        /// <param name="designDoc">The view to compact</param>
        /// <returns></returns>
        /// <remarks>Requires admin permissions.</remarks>
        public async Task<JObject> CompactAsync(string designDoc)
        {
            var response = await (await GetRequestAsync(_databaseBaseUri + "/_compact/" + designDoc).ConfigureAwait(false))
                .Post()
                .Json()
                .GetCouchResponseAsync().ConfigureAwait(false);

            return CheckAccepted(response);
        }

        public async Task<string> ShowAsync(string showName, string docId, string designDoc)
        {
            // TODO:  add in Etag support for Shows
            var uri = string.Format("{0}/_design/{1}/_show/{2}/{3}", _databaseBaseUri, designDoc, showName, docId);
            var request = await GetRequestAsync(uri).ConfigureAwait(false);
            return (await request.GetCouchResponseAsync().ConfigureAwait(false)).ResponseString;
        }

        public async Task<IListResult> ListAsync(string listName, string viewName, ViewOptions options, string designDoc)
        {
            var uri = string.Format("{0}/_design/{1}/_list/{2}/{3}{4}", _databaseBaseUri, designDoc, listName, viewName, options.ToString());
            var request = await GetRequestAsync(uri).ConfigureAwait(false);
            return new ListResult(request.GetRequest(), await request.GetCouchResponseAsync().ConfigureAwait(false));
        }

        private async Task<ViewResult<T>> ProcessGenericResultsAsync<T>(string uri, ViewOptions options) 
            where T : class
        {
            CouchRequest request = await GetRequestAsync(options, uri).ConfigureAwait(false);
            CouchResponse response = await request.GetCouchResponseAsync().ConfigureAwait(false);

            bool includeDocs = false;
            if (options != null)
                includeDocs = options.IncludeDocs ?? false;

            return new ViewResult<T>(response, request.GetRequest(), _objectSerializer, includeDocs);
        }

        /// <summary>
        /// Gets the results of the view using any and all parameters
        /// </summary>
        /// <param name="viewName">The name of the view</param>
        /// <param name="options">Options such as startkey etc.</param>
        /// <param name="designDoc">The design doc on which the view resides</param>
        /// <returns></returns>
        public Task<ViewResult<T>> ViewAsync<T>(string viewName, ViewOptions options, string designDoc) 
            where T : class
        {
            var uri = string.Format("{0}/_design/{1}/_view/{2}", _databaseBaseUri, designDoc, viewName);
            return ProcessGenericResultsAsync<T>(uri, options);
        }

        private async Task<ViewResult> ProcessResultsAsync(string uri, ViewOptions options)
        {
            CouchRequest request = await GetRequestAsync(options, uri).ConfigureAwait(false);
            CouchResponse response = await request.GetCouchResponseAsync().ConfigureAwait(false);
            return new ViewResult(response, request.GetRequest());
        }

        /// <summary>
        /// Updates security configuration for the database
        /// </summary>
        /// <param name="securityDoc"></param>
        public async Task UpdateSecurityDocumentAsync(SecurityDocument securityDoc)
        {
            string request = _databaseBaseUri + "/_security";

            // Serialize SecurityDocument to json
            string data = JsonConvert.SerializeObject(securityDoc);

            var result = await (await (await GetRequestAsync(request))
                .Put()
                .Json()
                .DataAsync(data))
                .GetCouchResponseAsync();

            // Check if okay
            if (result.StatusCode != HttpStatusCode.OK)
                throw new WebException("An error occurred while trying to update the security document. StatusDescription: " + result.StatusDescription);
        }

        /// <summary>
        /// Gets all the documents in the database using the _all_docs uri
        /// </summary>
        /// <returns></returns>
        public Task<ViewResult> GetAllDocumentsAsync()
        {
            var uri = _databaseBaseUri + "/_all_docs";
            return ProcessResultsAsync(uri, null);
        }

        public Task<ViewResult> GetAllDocumentsAsync(ViewOptions options)
        {
            var uri = _databaseBaseUri + "/_all_docs";
            return ProcessResultsAsync(uri, options);
        }
    }
}