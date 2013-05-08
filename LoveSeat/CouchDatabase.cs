using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
        public async Task<CouchResponse> CreateDocument(string id, string jsonForDocument)
        {
            var jobj = JObject.Parse(jsonForDocument);
            if (jobj.Value<object>("_rev") == null)
                jobj.Remove("_rev");

            var request = await GetRequest(databaseBaseUri + "/" + id).ConfigureAwait(false);
            var resp = await request.Put().Form().Data(jobj.ToString(Formatting.None)).GetResponse().ConfigureAwait(false);
            return resp.GetJObject();
        }

        public async Task<CouchResponse> CreateDocument(IBaseObject doc) 
        {
            var serialized = ObjectSerializer.Serialize(doc);
            if (doc.Id != null)
            {
                return await CreateDocument(doc.Id, serialized).ConfigureAwait(false);
            }
            return await CreateDocument(serialized).ConfigureAwait(false);
        }
        /// <summary>
        /// Creates a document when you intend for Couch to generate the id for you.
        /// </summary>
        /// <param name="jsonForDocument">Json for creating the document</param>
        /// <returns>The response as a JObject</returns>
        public async Task<CouchResponse> CreateDocument(string jsonForDocument)
        {
            JObject.Parse(jsonForDocument); //to make sure it's valid json

            var request = await GetRequest(databaseBaseUri + "/").ConfigureAwait(false);
            var response = await request.Post().Json().Data(jsonForDocument).GetResponse().ConfigureAwait(false);

            return response.GetJObject();
        }        
        public async Task<CouchResponse> DeleteDocument(string id, string rev)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(rev))
                throw new Exception("Both id and rev must have a value that is not empty");

            var request = await GetRequest(databaseBaseUri + "/" + id + "?rev=" + rev).ConfigureAwait(false);
            var response = await request.Delete().Form().GetResponse().ConfigureAwait(false);

            return response.GetJObject();
        }
        /// <summary>
        /// Returns null if document is not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<T> GetDocument<T>(string id, bool attachments, IObjectSerializer objectSerializer)
        {
            var request = await GetRequest(String.Format("{0}/{1}{2}", databaseBaseUri, id, attachments ? "?attachments=true" : string.Empty)).ConfigureAwait(false);
            var response = await request.Get().Json().GetResponse().ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotFound) return default(T);

            return objectSerializer.Deserialize<T>(response.GetResponseString());
        }
        public async Task<T> GetDocument<T>(string id, IObjectSerializer objectSerializer)
        {
            return await GetDocument<T>(id, false, objectSerializer).ConfigureAwait(false);
        }
        public async Task<T> GetDocument<T>(string id, bool attachments)
        {
            return await GetDocument<T>(id, attachments, ObjectSerializer).ConfigureAwait(false);
        }
        public async Task<T> GetDocument<T>(string id)
        {
            return await GetDocument<T>(id, false).ConfigureAwait(false);
        }
        public async Task<T> GetDocument<T>(Guid id, bool attachments, IObjectSerializer objectSerializer)
        {
            return await GetDocument<T>(id.ToString(), attachments, objectSerializer).ConfigureAwait(false);
        }
        public async Task<T> GetDocument<T>(Guid id, IObjectSerializer objectSerializer)
        {
            return await GetDocument<T>(id, false, objectSerializer).ConfigureAwait(false);
        }
        public async Task<T> GetDocument<T>(Guid id, bool attachments)
        {
            return await GetDocument<T>(id.ToString(), attachments).ConfigureAwait(false);
        }
        public async Task<T> GetDocument<T>(Guid id)
        {
            return await GetDocument<T>(id, false).ConfigureAwait(false);
        }
        public async Task<Document> GetDocument(string id, bool attachments)
        {
            var request = await GetRequest(String.Format("{0}/{1}{2}", databaseBaseUri, id, attachments ? "?attachments=true" : string.Empty)).ConfigureAwait(false);
            var response = await request.Get().Json().GetResponse().ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotFound) return null;
            return response.GetCouchDocument();
        }
        public async Task<Document> GetDocument(string id)
        {
            return await GetDocument(id, false).ConfigureAwait(false);
        }

        /// <summary>
        /// Request multiple documents 
        /// in a single request.
        /// </summary>
        /// <param name="keyLst"></param>
        /// <returns></returns>
        public async Task<ViewResult> GetDocuments(Keys keyLst)
        {
            // serialize list of keys to json
            string data = JsonConvert.SerializeObject(keyLst);

            var request = await GetRequest(databaseBaseUri + "/_all_docs").ConfigureAwait(false);
            var response = await request.Post().Json().Data(data).GetResponse().ConfigureAwait(false);

            if (response == null) return null;

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            return new ViewResult(response, null);
        }
 
        /// <summary>
        /// Using the bulk API for the loading of documents.
        /// </summary>
        /// <param name="docs"></param>
        /// <remarks>Here we assume you have either added the correct rev, id, or _deleted attribute to each document.  The response will indicate if there were any errors.
        /// Please note that the max_document_size configuration variable in CouchDB limits the maximum request size to CouchDB.</remarks>
        /// <returns>JSON of updated documents in the BulkDocumentResponse class.  </returns>
        public async Task<BulkDocumentResponses> SaveDocuments(Documents docs, bool all_or_nothing)
        {
            string uri = databaseBaseUri + "/_bulk_docs";

            string data = JsonConvert.SerializeObject(docs);

            if (all_or_nothing == true)
            {
                uri = uri + "?all_or_nothing=true";
            }

            var request = await GetRequest(uri).ConfigureAwait(false);
            var response = await request.Post().Json().Data(data).GetResponse().ConfigureAwait(false);

            if (response == null)
            {
                throw new System.Exception("Response returned null.");
            }

            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new System.Exception("Response returned with a HTTP status code of " + response.StatusCode + " - " + response.StatusDescription);    
            }

            // Get response
            string x = response.GetResponseString();
                        
            // Convert to Bulk response
            return JsonConvert.DeserializeObject<BulkDocumentResponses>(x);
        }

        
        /// <summary>
        /// Adds an attachment to a document.  If revision is not specified then the most recent will be fetched and used.  Warning: if you need document update conflicts to occur please use the method that specifies the revision
        /// </summary>
        /// <param name="id">id of the couch Document</param>
        /// <param name="attachment">byte[] of of the attachment.  Use File.ReadAllBytes()</param>
        /// <param name="filename">filename of the attachment</param>
        /// <param name="contentType">Content Type must be specifed</param>	
        public async Task<CouchResponse> AddAttachment(string id, byte[] attachment, string filename, string contentType)
        {
            var doc = await GetDocument(id).ConfigureAwait(false);
            return await AddAttachment(id, doc.Rev, attachment, filename, contentType).ConfigureAwait(false);
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
        public async Task<CouchResponse> AddAttachment(string id, string rev, byte[] attachment, string filename, string contentType)
        {
            var request = await GetRequest(string.Format("{0}/{1}/{2}?rev={3}", databaseBaseUri, id, filename, rev)).ConfigureAwait(false);
            var response = await request.Put().ContentType(contentType).Data(attachment).GetResponse().ConfigureAwait(false);

            return response.GetJObject();
        }
        /// <summary>
        /// Adds an attachment to a document.  If revision is not specified then the most recent will be fetched and used.  Warning: if you need document update conflicts to occur please use the method that specifies the revision
        /// </summary>
        /// <param name="id">id of the couch Document</param>
        /// <param name="attachmentStream">Stream of the attachment.</param>
        /// <param name="contentType">Content Type must be specifed</param>	
        public async Task<CouchResponse> AddAttachment(string id, Stream attachmentStream, string filename, string contentType)
        {
            var doc = await GetDocument(id).ConfigureAwait(false);
            return await AddAttachment(id, doc.Rev, attachmentStream, filename, contentType).ConfigureAwait(false);
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
        public async Task<CouchResponse> AddAttachment(string id, string rev, Stream attachmentStream, string filename, string contentType)
        {
            var request = await GetRequest(string.Format("{0}/{1}/{2}?rev={3}", databaseBaseUri, id, filename, rev)).ConfigureAwait(false);
            request = await request.Put().ContentType(contentType).Data(attachmentStream).ConfigureAwait(false);

            var response = await request.GetResponse().ConfigureAwait(false);

            return response.GetJObject();
        }

        public async Task<Stream> GetAttachmentStream(Document doc, string attachmentName)
        {
            return await GetAttachmentStream(doc.Id, doc.Rev, attachmentName).ConfigureAwait(false);
        }

        public async Task<Stream> GetAttachmentStream(string docId, string rev, string attachmentName)
        {
            var request = await GetRequest(string.Format("{0}/{1}/{2}", databaseBaseUri, docId, HttpUtility.UrlEncode(attachmentName))).ConfigureAwait(false);
            var response = await request.Get().GetResponse().ConfigureAwait(false);
            
            return response.GetResponseStream();
        }

        public async Task<Stream> GetAttachmentStream(string docId, string attachmentName)
        {
            var doc = await GetDocument(docId).ConfigureAwait(false);
            if (doc == null) return null;
            return await GetAttachmentStream(docId, doc.Rev, attachmentName).ConfigureAwait(false);
        }

        public async Task<CouchResponse> DeleteAttachment(string id, string rev, string attachmentName)
        {
            var request = await GetRequest(string.Format("{0}/{1}/{2}?rev={3}", databaseBaseUri, id, attachmentName, rev)).ConfigureAwait(false);
            var response = await request.Json().Delete().GetResponse().ConfigureAwait(false);
            
            return response.GetJObject();
        }
        public async Task<CouchResponse> DeleteAttachment(string id, string attachmentName)
        {
            var doc = await GetDocument(id).ConfigureAwait(false);
            return await DeleteAttachment(doc.Id, doc.Rev, attachmentName).ConfigureAwait(false);
        }

        public async Task<CouchResponse> SaveDocument(Document document)
        {
            if (document.Rev == null)
                return await CreateDocument(document).ConfigureAwait(false);

            var request = await GetRequest(string.Format("{0}/{1}?rev={2}", databaseBaseUri, document.Id, document.Rev)).ConfigureAwait(false);
            var response = await request.Put().Form().Data(document).GetResponse().ConfigureAwait(false);

            return response.GetJObject();
        }

        /// <summary>
        /// Gets the results of a view with no view parameters.  Use the overload to pass parameters
        /// </summary>
        /// <param name="viewName">The name of the view</param>
        /// <param name="designDoc">The design doc on which the view resides</param>
        /// <returns></returns>
        public async Task<ViewResult<T>> View<T>(string viewName, string designDoc)
        {
            return await View<T>(viewName, null, designDoc).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the results of the view using the defaultDesignDoc and no view parameters.  Use the overloads to specify options.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public async Task<ViewResult<T>> View<T>(string viewName)
        {
            ThrowDesignDocException();
            return await View<T>(viewName, defaultDesignDoc).ConfigureAwait(false);
        }
        public async Task<ViewResult> View(string viewName)
        {
            ThrowDesignDocException();
            return await View(viewName, new ViewOptions()).ConfigureAwait(false);
        }

        /// <summary>
        /// Call view cleanup for a database
        /// </summary>
        /// <returns>JSON success statement if the response code is Accepted</returns>
        public async Task<JObject> ViewCleanup()
        {
            return await DoCommand("_view_cleanup").ConfigureAwait(false);
        }

        private async Task<JObject> DoCommand(string command)
        {
            var request = await GetRequest(string.Format("{0}/{1}", databaseBaseUri, command)).ConfigureAwait(false);
            var response = await request.Post().Json().GetResponse().ConfigureAwait(false);

            return CheckAccepted(response);
        }

        /// <summary>
        /// Compact the current database
        /// </summary>
        /// <returns></returns>
        public async Task<JObject> Compact()
        {
            return await DoCommand("_compact").ConfigureAwait(false);
        }

        /// <summary>
        /// Compact a view.
        /// </summary>
        /// <param name="designDoc">The view to compact</param>
        /// <returns></returns>
        /// <remarks>Requires admin permissions.</remarks>
        public async Task<JObject> Compact(string designDoc)
        {
            return await DoCommand("_compact/" + designDoc).ConfigureAwait(false);
        }

        private static JObject CheckAccepted(HttpWebResponse resp)
        {
            if (resp == null) {
                throw new Exception("Response returned null.");
            }

            if (resp.StatusCode != HttpStatusCode.Accepted) {
                throw new Exception(string.Format("Response return with a HTTP Code of {0} - {1}", resp.StatusCode, resp.StatusDescription));
            }

            return resp.GetJObject();

        }


        public async Task<string> Show(string showName, string docId)
        {
            ThrowDesignDocException();
            return await Show(showName, docId, defaultDesignDoc).ConfigureAwait(false);
        }

        private void ThrowDesignDocException()
        {
            if (string.IsNullOrEmpty(defaultDesignDoc))
                throw new Exception("You must use SetDefaultDesignDoc prior to using this signature.  Otherwise explicitly specify the design doc in the other overloads.");
        }

        public async Task<string> Show(string showName, string docId, string designDoc)
        {
            //TODO:  add in Etag support for Shows
            var uri = string.Format("{0}/_design/{1}/_show/{2}/{3}", databaseBaseUri, designDoc, showName, docId);
            var request = await GetRequest(uri).ConfigureAwait(false);
            var response = await request.GetResponse().ConfigureAwait(false);
            
            return response.GetResponseString();
        }
        public async Task<IListResult> List(string listName, string viewName, ViewOptions options, string designDoc)
        {            
			var uri = string.Format("{0}/_design/{1}/_list/{2}/{3}{4}", databaseBaseUri, designDoc, listName, viewName, options.ToString());

            var request = await GetRequest(uri).ConfigureAwait(false);
            return new ListResult(request.GetRequest(), await request.GetResponse().ConfigureAwait(false));
        }

        public async Task<IListResult> List(string listName, string viewName, ViewOptions options)
        {
            ThrowDesignDocException();
            return await List(listName, viewName, options, defaultDesignDoc).ConfigureAwait(false);
        }

        public void SetDefaultDesignDoc(string designDoc)
        {
            this.defaultDesignDoc = designDoc;
        }

        private async Task<ViewResult<T>> ProcessGenericResults<T>(string uri, ViewOptions options) {
            var req = await GetRequest(options, uri).ConfigureAwait(false);
            var resp = await req.GetResponse().ConfigureAwait(false);
            if (resp.StatusCode == HttpStatusCode.BadRequest) {
                throw new CouchException(req.GetRequest(), resp, resp.GetResponseString() + "\n" + req.GetRequest().RequestUri);
            }

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
        public async Task<ViewResult<T>> View<T>(string viewName, ViewOptions options, string designDoc)
        {
            var uri = string.Format("{0}/_design/{1}/_view/{2}", databaseBaseUri, designDoc, viewName);
            return await ProcessGenericResults<T>(uri, options).ConfigureAwait(false);
        }
        /// <summary>
        /// Allows you to specify options and uses the defaultDesignDoc Specified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewName"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<ViewResult<T>> View<T>(string viewName, ViewOptions options)
        {
            ThrowDesignDocException();
            return await View<T>(viewName, options, defaultDesignDoc).ConfigureAwait(false);
        }

        public async Task<ViewResult> View(string viewName, ViewOptions options, string designDoc)
        {
            var uri = string.Format("{0}/_design/{1}/_view/{2}", databaseBaseUri, designDoc, viewName);
            return await ProcessResults(uri, options).ConfigureAwait(false);
        }

        public async Task<ViewResult> View(string viewName, ViewOptions options)
        {
            ThrowDesignDocException();
            return await View(viewName, options, this.defaultDesignDoc).ConfigureAwait(false);
        }
        private async Task<ViewResult> ProcessResults(string uri, ViewOptions options)
        {
            var req = await GetRequest(options, uri).ConfigureAwait(false);
            var resp = await req.GetResponse().ConfigureAwait(false);
            return new ViewResult(resp, req.GetRequest());
        }
        
        private async Task<CouchRequest> GetRequest(ViewOptions options, string uri)
        {
            if (options != null)
                uri +=  options.ToString();

            var request = await GetRequest(uri, options == null ? null : options.Etag).ConfigureAwait(false);
            return request.Get().Json();
        }


        /// <summary>
        /// Gets all the documents in the database using the _all_docs uri
        /// </summary>
        /// <returns></returns>
        public async Task<ViewResult> GetAllDocuments()
        {
            var uri = databaseBaseUri + "/_all_docs";
            return await ProcessResults(uri, null).ConfigureAwait(false);
        }
        public async Task<ViewResult> GetAllDocuments(ViewOptions options)
        {
            var uri = databaseBaseUri + "/_all_docs";
            return await ProcessResults(uri, options).ConfigureAwait(false);
        }




        #region Security
        public async Task<SecurityDocument> getSecurityConfiguration()
        {
            var request = await GetRequest(databaseBaseUri + "/_security").ConfigureAwait(false);
            var response = await request.Get().Json().GetResponse().ConfigureAwait(false);
            
            var docResult = response.GetJObject();

            return JsonConvert.DeserializeObject<SecurityDocument>(docResult.ToString());
        }

        /// <summary>
        /// Updates security configuration for the database
        /// </summary>
        /// <param name="sDoc"></param>
        public async Task UpdateSecurityDocument(SecurityDocument sDoc)
        {
            // serialize SecurityDocument to json
            string data = JsonConvert.SerializeObject(sDoc);

            var request = await GetRequest(databaseBaseUri + "/_security").ConfigureAwait(false);
            var result = await request.Put().Json().Data(data).GetResponse().ConfigureAwait(false);

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