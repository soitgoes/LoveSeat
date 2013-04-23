using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using LoveSeat.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using LoveSeat;

namespace LoveSeat.IntegrationTest
{
    [TestFixture]
	public class CouchClientTest
	{
		private CouchClient client;
		private const string baseDatabase = "love-seat-test-base";
        private const string replicateDatabase = "love-seat-test-repli";

		private readonly string host = ConfigurationManager.AppSettings["Host"];
		private readonly int port = int.Parse(ConfigurationManager.AppSettings["Port"]);
		private readonly string username = ConfigurationManager.AppSettings["UserName"];
		private readonly string password = ConfigurationManager.AppSettings["Password"];

		[SetUp]
		public void Setup()
		{
			client = new CouchClient(host, port, username, password, false,AuthenticationType.Cookie);
			if (!client.HasDatabase(baseDatabase))
			{
				client.CreateDatabase(baseDatabase);
			}
            if (!client.HasDatabase(replicateDatabase))
            {
                client.CreateDatabase(replicateDatabase);
            }
		}
		[TearDown]
		public void TearDown()
		{
            //delete the test database
            if (client.HasDatabase(baseDatabase)) {
                client.DeleteDatabase(baseDatabase);
            }
            if (client.HasDatabase(replicateDatabase)) {
                client.DeleteDatabase(replicateDatabase);
            }
            if (client.HasUser("Leela")) {
                client.DeleteAdminUser("Leela");
            }
		}

		[Test]
		public void Should_Trigger_Replication()
		{
			var obj  = client.TriggerReplication("http://Professor:Farnsworth@"+ host+":5984/" +replicateDatabase, baseDatabase);
			Assert.IsTrue(obj != null);
		}
        public class Bunny {
            public Bunny() { }
            public string Name { get; set; }
        }
        [Test]
        public void Creating_A_Document_Should_Keep_Id_If_Supplied()
        {
            var doc = new Document<Bunny>(new Bunny());
            doc.Id = "myid";
            var db = client.GetDatabase(baseDatabase);
            db.CreateDocument(doc);
            var savedDoc = db.GetDocument("myid");
            Assert.IsNotNull(savedDoc, "Saved doc should be able to be retrieved by the same id");
        }

		[Test]
		public void Should_Create_Document_From_String()
		{
			string obj = @"{""test"": ""prop""}";
			var db = client.GetDatabase(baseDatabase);
			var result = db.CreateDocument("fdas", obj);
		    var document = db.GetDocument("fdas");
			Assert.IsNotNull(document);
		}
        [Test]
        public void Should_Save_Existing_Document()
        {
            string obj = @"{""test"": ""prop""}";
            var db = client.GetDatabase(baseDatabase);
            var result = db.CreateDocument("fdas", obj);
            var doc = db.GetDocument("fdas");
            doc["test"] = "newprop";
            db.SaveDocument(doc);
            var newresult= db.GetDocument("fdas");
            Assert.AreEqual(newresult.Value<string>("test"), "newprop");
        }

		[Test]
		public void Should_Delete_Document()
		{
			var db = client.GetDatabase(baseDatabase);
			db.CreateDocument("asdf", "{}");
			var doc = db.GetDocument("asdf");
			var result = 	db.DeleteDocument(doc.Id, doc.Rev);
			Assert.IsNull(db.GetDocument("asdf"));
		}


		[Test]
		public void Should_Determine_If_Doc_Has_Attachment()
		{
			var db = client.GetDatabase(baseDatabase);
			db.CreateDocument(@"{""_id"":""fdsa""}");
			byte[] attachment = File.ReadAllBytes("../../Files/martin.jpg");
			db.AddAttachment("fdsa" , attachment,"martin.jpg", "image/jpeg");
			var doc = db.GetDocument("fdsa");
			Assert.IsTrue(doc.HasAttachment);
		}
		[Test]
		public void Should_Return_Attachment_Names()
		{
			var db = client.GetDatabase(baseDatabase);
			db.CreateDocument(@"{""_id"":""upload""}");
			var attachment = File.ReadAllBytes("../../Files/martin.jpg");
			db.AddAttachment("upload", attachment,  "martin.jpg", "image/jpeg");
			var doc = db.GetDocument("upload");
			Assert.IsTrue(doc.GetAttachmentNames().Contains("martin.jpg"));
		}

		[Test]
		public void Should_Create_Admin_User()
		{			
			client.CreateAdminUser("Leela", "Turanga");
		}

		[Test]
		public void Should_Delete_Admin_User()
		{
			client.DeleteAdminUser("Leela");
		}

		[Test]
		public void Should_Get_Attachment()
		{
			var db = client.GetDatabase(baseDatabase);
			db.CreateDocument(@"{""_id"":""test_upload""}");
			var doc = db.GetDocument("test_upload");
			var attachment = File.ReadAllBytes("../../Files/test_upload.txt");
			db.AddAttachment("test_upload", attachment, "test_upload.txt", "text/html");
			var stream = db.GetAttachmentStream(doc, "test_upload.txt");
			using (StreamReader sr = new StreamReader(stream))
			{
				string result = sr.ReadToEnd();
				Assert.IsTrue(result == "test");	
			}			
		}
		[Test]
		public void Should_Delete_Attachment()
		{
			var db = client.GetDatabase(baseDatabase);
			db.CreateDocument(@"{""_id"":""test_delete""}");
			var doc = db.GetDocument("test_delete");
			var attachment = File.ReadAllBytes("../../Files/test_upload.txt");
			db.AddAttachment("test_delete", attachment, "test_upload.txt", "text/html");
			db.DeleteAttachment("test_delete", "test_upload.txt");
			var retrieved = db.GetDocument("test_delete");
			Assert.IsFalse(retrieved.HasAttachment);
		}
        [Test]
        public void Should_Return_Etag_In_ViewResults()
        {
            var db = client.GetDatabase(baseDatabase);
            db.CreateDocument(@"{""_id"":""test_eTag""}");
            ViewResult result = db.GetAllDocuments();
           Assert.IsTrue(!string.IsNullOrEmpty(result.Etag));
        }

        [Test]
        public void Should_Persist_Property()
        {
            var db = client.GetDatabase(baseDatabase);
            var company = new Company();
            company.Id = "1234";
            company.Name = "Whiteboard-IT";
            db.CreateDocument(company);
            var doc = db.GetDocument<Company>("1234");
            Assert.AreEqual(company.Name, doc.Name);
        }
        [Test]
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
            Assert.IsTrue(result.Contains("Whiteboard-it"));
        }
	    [Test]
	    public void Should_Get_304_If_ETag_Matches()
	    {
            var db = client.GetDatabase(baseDatabase);
            db.CreateDocument(@"{""_id"":""test_eTag_exception""}");
            ViewResult result = db.GetAllDocuments();
	        ViewResult cachedResult = db.GetAllDocuments(new ViewOptions {Etag = result.Etag});
            Assert.AreEqual(cachedResult.StatusCode, HttpStatusCode.NotModified);
	    } 
        [Test]
        public void Should_Get_Id_From_Existing_Document()
        {
            var db = client.GetDatabase(baseDatabase);
            string id = "test_should_get_id";
            db.CreateDocument("{\"_id\":\""+ id+"\"}");
            Document doc= db.GetDocument(id);
            Assert.AreEqual(id, doc.Id);
        }

        [Test]
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

            var db = client.GetDatabase(baseDatabase);
            db.CreateDocument(doc._id, JsonConvert.SerializeObject(doc, Formatting.Indented, settings));

            var company = new Company();
            company.Name = "foo";
            db.CreateDocument(company);

            // Without IncludeDocs
            Assert.IsNull(db.View<Company>(viewName, designDoc).Items.ToList()[0]);

            // With IncludeDocs
            ViewOptions options = new ViewOptions { IncludeDocs = true };
            Assert.AreEqual("foo", db.View<Company>(viewName, options, designDoc).Items.ToList()[0].Name);
        }
	}
    public class Company : IBaseObject
    {        
        public string Name { get; set; }
        public string Id { get; set; }
        public string Rev { get; set; }
        public string Type { get { return "company"; } }
    }
}
