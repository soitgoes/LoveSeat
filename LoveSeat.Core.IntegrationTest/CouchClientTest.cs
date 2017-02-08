using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using LoveSeat.Core.Interfaces;
using Xunit;

namespace LoveSeat.Core.IntegrationTest
{
    public class CouchClientTest : IDisposable
    {
        private CouchClient _client;
        private const string _baseDatabase = "love-seat-test-base";
        private const string _replicateDatabase = "love-seat-test-repli";

        private string _host = "localhost";
        private int _port = 5984;
        private string _username = "admin";
        private string _password = "pass";

        public CouchClientTest()
        {
            _client = new CouchClient(_host, _port, _username, _password, false, AuthenticationType.Cookie);
            if (!_client.HasDatabase(_baseDatabase))
                _client.CreateDatabase(_baseDatabase);

            if (!_client.HasDatabase(_replicateDatabase))
                _client.CreateDatabase(_replicateDatabase);
        }

        [Fact]
        public void Should_Trigger_Replication()
        {
            var obj = _client.TriggerReplication("http://" + _username + _password + "@" + _host + ":5984/" + _replicateDatabase, _baseDatabase);
            Assert.True(obj != null);
        }

        [Fact]
        public void Creating_A_Document_Should_Keep_Id_If_Supplied()
        {
            var doc = new Document<Bunny>(new Bunny());
            doc.Id = "myid";
            var db = _client.GetDatabase(_baseDatabase);
            db.CreateDocument(doc);
            var savedDoc = db.GetDocument("myid");
            Assert.NotNull(savedDoc);
        }

        [Fact]
        public void Should_Create_Document_From_String()
        {
            string obj = @"{""test"": ""prop""}";
            var db = _client.GetDatabase(_baseDatabase);
            var result = db.CreateDocument("fdas", obj);
            var document = db.GetDocument("fdas");
            Assert.NotNull(document);
        }

        [Fact]
        public void Should_Save_Existing_Document()
        {
            string obj = @"{""test"": ""prop""}";
            var db = _client.GetDatabase(_baseDatabase);
            var result = db.CreateDocument("fdas", obj);
            var doc = db.GetDocument("fdas");
            doc.JObject["test"] = "newprop";
            db.SaveDocument(doc);
            var newresult = db.GetDocument("fdas");
            Assert.Equal(newresult.JObject.Value<string>("test"), "newprop");
        }

        [Fact]
        public void Should_Delete_Document()
        {
            var db = _client.GetDatabase(_baseDatabase);
            db.CreateDocument("asdf", "{}");
            var doc = db.GetDocument("asdf");
            var result = db.DeleteDocument(doc.Id, doc.Rev);
            Assert.Null(db.GetDocument("asdf"));
        }

        [Fact]
        public void Should_Save_And_Retrieve_A_Document_With_Generics()
        {
            var id = Guid.NewGuid().ToString();
            var db = _client.GetDatabase(_baseDatabase);
            var bunny = new Bunny { Id = id, Name = "Hippity Hop" };
            var doc = new Document<Bunny>(bunny);

            var attachment = GetAttachment(AttachmentType.Image);
            doc.AddAttachment("Penguins.jpg", attachment);
            db.SaveDocument(doc);
            var persistedBunny = db.GetDocument<Bunny>(id);
            Assert.True(persistedBunny.HasAttachment);
        }

        [Fact]
        public void Should_Determine_If_Doc_Has_Attachment()
        {
            var db = _client.GetDatabase(_baseDatabase);
            db.CreateDocument(@"{""_id"":""fdsa""}");
            var attachment = GetAttachment(AttachmentType.Image);
            db.AddAttachment("fdsa", attachment, "Penguins.jpg", "image/jpeg");
            var doc = db.GetDocument("fdsa");
            Assert.True(doc.HasAttachment);
        }

        [Fact]
        public void Should_Return_Attachment_Names()
        {
            var db = _client.GetDatabase(_baseDatabase);
            db.CreateDocument(@"{""_id"":""upload""}");
            var attachment = GetAttachment(AttachmentType.Image);
            db.AddAttachment("upload", attachment, "Penguins.jpg", "image/jpeg");
            var doc = db.GetDocument("upload");
            Assert.True(doc.GetAttachmentNames().Contains("Penguins.jpg"));
        }

        [Fact]
        public void Should_Return_Docs_On_View()
        {
            var db = _client.GetDatabase(_baseDatabase);
            var jobj = JObject.Parse("{\"_id\": \"_design/Patient\",\"views\": {\"all\": {\"map\": \"function (doc) {\n                 emit(doc._id, null);\n             }\"}},\"type\": \"designdoc\"}");
            db.CreateDocument(jobj.ToString());
            var bunny = new Bunny { Name = "Roger" };
            db.CreateDocument(new Document<Bunny>(bunny));
            var result = db.View("all", new ViewOptions { IncludeDocs = true }, "Patient");
            Assert.True(result.Docs.Any());
        }

        [Fact]
        public void Should_Create_Admin_User()
        {
            _client.CreateAdminUser("Leela", "Turanga");
        }

        [Fact]
        public void Should_Delete_Admin_User()
        {
            _client.DeleteAdminUser("Leela");
        }

        [Fact]
        public void Should_Get_Attachment()
        {
            var db = _client.GetDatabase(_baseDatabase);
            db.CreateDocument(@"{""_id"":""test_upload""}");
            var doc = db.GetDocument("test_upload");
            var attachment = GetAttachment(AttachmentType.File);
            db.AddAttachment("test_upload", attachment, "test_upload.txt", "text/html");
            var stream = db.GetAttachmentStream(doc, "test_upload.txt");
            using (StreamReader sr = new StreamReader(stream))
            {
                string result = sr.ReadToEnd();
                Assert.True(result == "test");
            }
        }

        [Fact]
        public void Should_Delete_Attachment()
        {
            var db = _client.GetDatabase(_baseDatabase);
            db.CreateDocument(@"{""_id"":""test_delete""}");
            var doc = db.GetDocument("test_delete");
            var attachment = GetAttachment(AttachmentType.File);
            db.AddAttachment("test_delete", attachment, "test_upload.txt", "text/html");
            db.DeleteAttachment("test_delete", "test_upload.txt");
            var retrieved = db.GetDocument("test_delete");
            Assert.False(retrieved.HasAttachment);
        }

        [Fact]
        public void Should_Return_Etag_In_ViewResults()
        {
            var db = _client.GetDatabase(_baseDatabase);
            db.CreateDocument(@"{""_id"":""test_eTag""}");
            ViewResult result = db.GetAllDocuments();
            Assert.True(!string.IsNullOrEmpty(result.Etag));
        }

        [Fact]
        public void Should_Persist_Property()
        {
            var db = _client.GetDatabase(_baseDatabase);
            var company = new Company();
            company.Id = "1234";
            company.Name = "Whiteboard-IT";
            db.CreateDocument(company);
            var doc = db.GetDocument<Company>("1234");
            Assert.Equal(company.Name, doc.Item.Name);
        }

        [Fact]
        public void JsonConvert_Should_Serialize_Properly()
        {
            var company = new Company();
            company.Name = "Whiteboard-it";
            var settings = new JsonSerializerSettings();
            var converters = new List<JsonConverter> { new IsoDateTimeConverter() };
            settings.Converters = converters;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.NullValueHandling = NullValueHandling.Ignore;
            var result = JsonConvert.SerializeObject(company, Formatting.Indented, settings);
            Console.Write(result);
            Assert.True(result.Contains("Whiteboard-it"));
        }

        [Fact]
        public void Should_Get_304_If_ETag_Matches()
        {
            var db = _client.GetDatabase(_baseDatabase);
            db.CreateDocument(@"{""_id"":""test_eTag_exception""}");
            ViewResult result = db.GetAllDocuments();
            ViewResult cachedResult = db.GetAllDocuments(new ViewOptions { Etag = result.Etag });
            Assert.Equal(cachedResult.StatusCode, HttpStatusCode.NotModified);
        }

        [Fact]
        public void Should_Get_Id_From_Existing_Document()
        {
            var db = _client.GetDatabase(_baseDatabase);
            string id = "test_should_get_id";
            db.CreateDocument("{\"_id\":\"" + id + "\"}");
            Document doc = db.GetDocument(id);
            Assert.Equal(id, doc.Id);
        }

        [Fact]
        public void Should_Populate_Items_When_IncludeDocs_Set_In_ViewOptions()
        {
            string designDoc = "test";
            string viewName = "testView";
            var settings = new JsonSerializerSettings();
            var converters = new List<JsonConverter> { new IsoDateTimeConverter() };
            settings.Converters = converters;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.NullValueHandling = NullValueHandling.Ignore;

            var doc = new
            {
                _id = "_design/" + designDoc,
                Language = "javascript",
                Views = new
                {
                    TestView = new
                    {
                        Map = "function(doc) {\n  if(doc.type == 'company') {\n    emit(doc._id, null);\n  }\n}"
                    }
                }
            };

            var db = _client.GetDatabase(_baseDatabase);
            db.CreateDocument(doc._id, JsonConvert.SerializeObject(doc, Formatting.Indented, settings));

            var company = new Company();
            company.Name = "foo";
            db.CreateDocument(company);

            // Without IncludeDocs
            var items = db.View<Company>(viewName, designDoc).Items;

            Assert.Null(items.ToList()[0]);

            // With IncludeDocs
            ViewOptions options = new ViewOptions { IncludeDocs = true };

            items = db.View<Company>(viewName, options, designDoc).Items;
            Assert.True(items.Any(a => a.Name == "foo"));
        }

        private byte[] GetAttachment(AttachmentType type)
        {
            if (type == AttachmentType.File)
                return File.ReadAllBytes(Directory.GetCurrentDirectory() + "/Files/test_upload.txt");

            return File.ReadAllBytes(Directory.GetCurrentDirectory() + "/Files/penguins.jpg");
        }

        public void Dispose()
        {
            // delete the test database
            if (_client.HasDatabase(_baseDatabase))
                _client.DeleteDatabase(_baseDatabase);

            if (_client.HasDatabase(_replicateDatabase))
                _client.DeleteDatabase(_replicateDatabase);

            if (_client.HasUser("Leela"))
                _client.DeleteAdminUser("Leela");
        }

        enum AttachmentType
        {
            Image,
            File
        }
    }

    public class Company : IBaseObject
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Rev { get; set; }
        public string Type { get { return "company"; } }
    }

    public class Bunny : IBaseObject
    {
        public Bunny() { }
        public string Name { get; set; }
        public string Id { get; set; }
        public string Rev { get; set; }
        public string Type { get { return "bunny"; } }
    }
}
