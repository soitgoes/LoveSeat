using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using LoveSeat.Interfaces;
using LoveSeat.Support;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoveSeat.Cloudant
{
    public interface ICloudantDatabase : IDocumentDatabase
    {
        GenerateApiKeyResponse GenerateApiKey();
    }

    public class CloudantDatabase : ICloudantDatabase
    {
        private IDocumentDatabase database;
        private readonly ICouchConnection connection;
        private readonly ICouchFactory couchFactory;
        private readonly Uri cloudantApiV2DatabaseBaseUri;
        private readonly Uri cloudantApiV2BaseUri;

        public CloudantDatabase(IDocumentDatabase database, ICouchConnection connection, ICouchFactory couchFactory)
        {
            this.database = database;
            this.connection = connection;
            this.couchFactory = couchFactory;
            this.cloudantApiV2BaseUri = connection.BaseUri.Combine("_api/v2/"); 
            this.cloudantApiV2DatabaseBaseUri = cloudantApiV2BaseUri.Combine("db/" + connection.DatabaseName);

        }


        /// <summary>
        /// Get Security Document
        /// </summary>
        public object GetSecurityDocument()
        {
            var uri = cloudantApiV2DatabaseBaseUri.Combine("/_security");

            var result = couchFactory.GetRequest(uri, connection).Get().Json().GetCouchResponse();

            if (result.StatusCode != HttpStatusCode.OK) //Check if okay
            {
                throw new WebException("An error occurred while trying to get the security document. StatusDescription: " + result.StatusDescription);
            }

            return JsonConvert.DeserializeObject<CloudantSecurityDocument>(result.ResponseString);
        }

        public ViewResult GetDocuments(Keys keyLst)
        {
            return database.GetDocuments(keyLst);
        }

        /// <summary>
        /// Updates security configuration for the database
        /// </summary>
        /// <param name="sDoc"></param>
        public void UpdateSecurityDocument(object sDoc)
        {
            var uri = cloudantApiV2DatabaseBaseUri.Combine("/_security");

            // serialize SecurityDocument to json
            string data = JsonConvert.SerializeObject(sDoc);

            var result = couchFactory.GetRequest(uri, connection).Put().Json().Data(data).GetCouchResponse();

            if (result.StatusCode != HttpStatusCode.OK) //Check if okay
            {
                throw new WebException("An error occurred while trying to update the security document. StatusDescription: " + result.StatusDescription);
            }

            var simpleResponse = JsonConvert.DeserializeObject<SimpleResponse>(result.ResponseString);
            if (!simpleResponse.Ok)
            {
                throw new WebException("An error occurred while trying to update the security document.");
            }
        }


        public GenerateApiKeyResponse GenerateApiKey()
        {
            var uri = cloudantApiV2BaseUri.Combine("/api_keys");

            var request = couchFactory.GetRequest(uri, connection).Post();

            var resp = request.GetCouchResponse();


            var response = JsonConvert.DeserializeObject<GenerateApiKeyResponse>(resp.ResponseString);

            return response;
        }



        #region Search        
        public ICouchRequest SearchRequest(string query, string designDoc, string index)
        {
            var uri = database.DatabaseBaseUri.Combine(string.Format("_design/{1}/_search/{2}?{3}", designDoc, index, query));

            var request = couchFactory.GetRequest(uri, connection);
            return request;
        }
        #endregion


        #region IDocumentDatabase Pass thrus
        public CouchResponseObject CreateDocument(string id, string jsonForDocument)
        {
            return database.CreateDocument(id, jsonForDocument);
        }

        public CouchResponseObject CreateDocument(IBaseObject doc)
        {
            return database.CreateDocument(doc);
        }

        public CouchResponseObject CreateDocument(string jsonForDocument)
        {
            return database.CreateDocument(jsonForDocument);
        }

        public CouchResponseObject DeleteDocument(string id, string rev)
        {
            return database.DeleteDocument(id, rev);
        }

        public Document GetDocument(string id)
        {
            return database.GetDocument(id);
        }

        public T GetDocument<T>(string id)
        {
            return database.GetDocument<T>(id);
        }

        public CouchResponseObject AddAttachment(string id, byte[] attachment, string filename, string contentType)
        {
            return database.AddAttachment(id, attachment, filename, contentType);
        }

        public CouchResponseObject AddAttachment(string id, string rev, byte[] attachment, string filename, string contentType)
        {
            return database.AddAttachment(id, rev, attachment, filename, contentType);
        }

        public Stream GetAttachmentStream(Document doc, string attachmentName)
        {
            return database.GetAttachmentStream(doc, attachmentName);
        }

        public Stream GetAttachmentStream(string docId, string rev, string attachmentName)
        {
            return database.GetAttachmentStream(docId, rev, attachmentName);
        }

        public Stream GetAttachmentStream(string docId, string attachmentName)
        {
            return database.GetAttachmentStream(docId, attachmentName);
        }

        public CouchResponseObject DeleteAttachment(string id, string rev, string attachmentName)
        {
            return database.DeleteAttachment(id, rev, attachmentName);
        }

        public CouchResponseObject DeleteAttachment(string id, string attachmentName)
        {
            return database.DeleteAttachment(id, attachmentName);
        }

        public CouchResponseObject SaveDocument(Document document)
        {
            return database.SaveDocument(document);
        }

        public BulkDocumentResponses SaveDocuments(Documents docs, bool all_or_nothing)
        {
            return database.SaveDocuments(docs, all_or_nothing);
        }

        public ViewResult<T> View<T>(string viewName, string designDoc)
        {
            return database.View<T>(viewName, designDoc);
        }

        public ViewResult<T> View<T>(string viewName, ViewOptions options, string designDoc)
        {
            return database.View<T>(viewName, options, designDoc);
        }

        public ViewResult GetAllDocuments()
        {
            return database.GetAllDocuments();
        }

        public ViewResult GetAllDocuments(ViewOptions options)
        {
            return database.GetAllDocuments(options);
        }

        public ViewResult<T> View<T>(string viewName)
        {
            return database.View<T>(viewName);
        }

        public ViewResult<T> View<T>(string viewName, ViewOptions options)
        {
            return database.View<T>(viewName, options);
        }

        public T GetDocument<T>(Guid id, IObjectSerializer objectSerializer)
        {
            return database.GetDocument<T>(id, objectSerializer);
        }

        public T GetDocument<T>(Guid id)
        {
            return database.GetDocument<T>(id);
        }

        public string Show(string showName, string docId)
        {
            return database.Show(showName, docId);
        }

        public IListResult List(string listName, string viewName, ViewOptions options, string designDoc)
        {
            return database.List(listName, viewName, options, designDoc);
        }

        public IListResult List(string listName, string viewName, ViewOptions options)
        {
            return database.List(listName, viewName, options);
        }

        public ViewResult View(string viewName, ViewOptions options, string designDoc)
        {
            return database.View(viewName, options, designDoc);
        }

        public ViewResult View(string viewName, ViewOptions options)
        {
            return database.View(viewName, options);
        }

        public ViewResult View(string viewName)
        {
            return database.View(viewName);
        }

        public void SetTimeout(int timeoutMs)
        {
            database.SetTimeout(timeoutMs);
        }

        public void SetDefaultDesignDoc(string designDoc)
        {
            database.SetDefaultDesignDoc(designDoc);
        }

        public Uri DatabaseBaseUri
        {
            get { return database.DatabaseBaseUri; }
        }

        public bool HasSessionExpired
        {
            get { return database.HasSessionExpired; }
        }



        public Task<T> GetDocumentAsync<T>(string id, bool attachments, IObjectSerializer objectSerializer)
        {
            return database.GetDocumentAsync<T>(id, attachments, objectSerializer);
        }


        public Task<T> GetDocumentAsync<T>(string id)
        {
            return database.GetDocumentAsync<T>(id);
        }

        public Task<T> GetDocumentAsync<T>(string id, bool attachments)
        {
            return database.GetDocumentAsync<T>(id, attachments);
        }

        public Task<T> GetDocumentAsync<T>(string id, IObjectSerializer objectSerializer)
        {
            return database.GetDocumentAsync<T>(id, objectSerializer);
        }

        public Task<T> GetDocumentAsync<T>(Guid id)
        {
            return database.GetDocumentAsync<T>(id);
        }

        public Task<T> GetDocumentAsync<T>(Guid id, bool attachments)
        {
            return database.GetDocumentAsync<T>(id, attachments);
        }

        public Task<Document> GetDocumentAsync(string id, bool attachments)
        {
            return database.GetDocumentAsync(id, attachments);
        }

        public Task<Document> GetDocumentAsync(string id)
        {
            return database.GetDocumentAsync(id);
        }
    }
    #endregion

    
        public class GenerateApiKeyResponse
        {
            [JsonProperty("ok")]
            public bool Ok { get; set; }
            [JsonProperty("key")]
            public string Key { get; set; }
            [JsonProperty("password")]
            public string Password { get; set; }
        }
}
    