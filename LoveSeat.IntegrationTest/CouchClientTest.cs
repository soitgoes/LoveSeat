using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
			if (!client.HasDatabase(baseDatabase).Result)
			{
				client.CreateDatabase(baseDatabase).Wait();
			}
            if (!client.HasDatabase(replicateDatabase).Result)
            {
                client.CreateDatabase(replicateDatabase).Wait();
            }
		}
		[TearDown]
		public void TearDown()
		{
            //delete the test database
            if (client.HasDatabase(baseDatabase).Result) {
                client.DeleteDatabase(baseDatabase).Wait();
            }
            if (client.HasDatabase(replicateDatabase).Result) {
                client.DeleteDatabase(replicateDatabase).Wait();
            }
            if (client.HasUser("Leela").Result) {
                client.DeleteAdminUser("Leela").Wait();
            }
		}

		[Test]
		public async Task Should_Trigger_Replication()
		{
			var obj  = await client.TriggerReplication("http://Professor:Farnsworth@"+ host+":5984/" +replicateDatabase, baseDatabase);
			Assert.IsTrue(obj != null);
		}
        public class Bunny {
            public Bunny() { }
            public string Name { get; set; }
        }
        [Test]
        public async Task Creating_A_Document_Should_Keep_Id_If_Supplied()
        {
            var doc = new Document<Bunny>(new Bunny());
            doc.Id = "myid";
            var db = client.GetDatabase(baseDatabase);
            await db.CreateDocument(doc);
            var savedDoc = await db.GetDocument("myid");
            Assert.IsNotNull(savedDoc, "Saved doc should be able to be retrieved by the same id");
        }

		[Test]
		public async Task Should_Create_Document_From_String()
		{
			string obj = @"{""test"": ""prop""}";
			var db = client.GetDatabase(baseDatabase);
			var result = await db.CreateDocument("fdas", obj);
		    var document = await db.GetDocument("fdas");
			Assert.IsNotNull(document);
		}
        [Test]
        public async Task Should_Save_Existing_Document()
        {
            string obj = @"{""test"": ""prop""}";
            var db = client.GetDatabase(baseDatabase);
            var result = await db.CreateDocument("fdas", obj);
            var doc = await db.GetDocument("fdas");
            doc["test"] = "newprop";
            await db.SaveDocument(doc);
            var newresult = await db.GetDocument("fdas");
            Assert.AreEqual(newresult.Value<string>("test"), "newprop");
        }

		[Test]
		public async Task Should_Delete_Document()
		{
			var db = client.GetDatabase(baseDatabase);
			await db.CreateDocument("asdf", "{}");
			var doc = await db.GetDocument("asdf");
			var result = await db.DeleteDocument(doc.Id, doc.Rev);
			Assert.IsNull(await db.GetDocument("asdf"));
		}


		[Test]
		public async Task Should_Determine_If_Doc_Has_Attachment()
		{
			var db = client.GetDatabase(baseDatabase);
			await db.CreateDocument(@"{""_id"":""fdsa""}");
			byte[] attachment = File.ReadAllBytes("../../Files/martin.jpg");
			await db.AddAttachment("fdsa" , attachment,"martin.jpg", "image/jpeg");
			var doc = await db.GetDocument("fdsa");
			Assert.IsTrue(doc.HasAttachment);
		}
		[Test]
		public async Task Should_Return_Attachment_Names()
		{
			var db = client.GetDatabase(baseDatabase);
			await db.CreateDocument(@"{""_id"":""upload""}");
			var attachment = File.ReadAllBytes("../../Files/martin.jpg");
			await db.AddAttachment("upload", attachment,  "martin.jpg", "image/jpeg");
			var doc = await db.GetDocument("upload");
			Assert.IsTrue(doc.GetAttachmentNames().Contains("martin.jpg"));
		}

		[Test]
		public async Task Should_Create_Admin_User()
		{			
			await client.CreateAdminUser("Leela", "Turanga");
		}

		[Test]
		public async Task Should_Delete_Admin_User()
		{
			await client.DeleteAdminUser("Leela");
		}

		[Test]
		public async Task Should_Get_Attachment()
		{
			var db = client.GetDatabase(baseDatabase);
			await db.CreateDocument(@"{""_id"":""test_upload""}");
			var doc = await db.GetDocument("test_upload");
			var attachment = File.ReadAllBytes("../../Files/test_upload.txt");
			await db.AddAttachment("test_upload", attachment, "test_upload.txt", "text/html");
			var stream = await db.GetAttachmentStream(doc, "test_upload.txt");
			using (var sr = new StreamReader(stream))
			{
				string result = sr.ReadToEnd();
				Assert.IsTrue(result == "test");	
			}			
		}
		[Test]
		public async Task Should_Delete_Attachment()
		{
			var db = client.GetDatabase(baseDatabase);
			await db.CreateDocument(@"{""_id"":""test_delete""}");
			var doc = await db.GetDocument("test_delete");
			var attachment = File.ReadAllBytes("../../Files/test_upload.txt");
			await db.AddAttachment("test_delete", attachment, "test_upload.txt", "text/html");
			await db.DeleteAttachment("test_delete", "test_upload.txt");
			var retrieved = await db.GetDocument("test_delete");
			Assert.IsFalse(retrieved.HasAttachment);
		}
        [Test]
        public async Task Should_Return_Etag_In_ViewResults()
        {
            var db = client.GetDatabase(baseDatabase);
            await db.CreateDocument(@"{""_id"":""test_eTag""}");
            ViewResult result =  await db.GetAllDocuments();
           Assert.IsTrue(!string.IsNullOrEmpty(result.Etag));
        }

        [Test]
        public async Task Should_Persist_Property()
        {
            var db = client.GetDatabase(baseDatabase);
            var company = new Company();
            company.Id = "1234";
            company.Name = "Whiteboard-IT";
            await db.CreateDocument(company);
            var doc = await db.GetDocument<Company>("1234");
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
	    public async Task Should_Get_304_If_ETag_Matches()
	    {
            var db = client.GetDatabase(baseDatabase);
            await db.CreateDocument(@"{""_id"":""test_eTag_exception""}");
            ViewResult result = await db.GetAllDocuments();
	        ViewResult cachedResult = await db.GetAllDocuments(new ViewOptions {Etag = result.Etag});
            Assert.AreEqual(cachedResult.StatusCode, HttpStatusCode.NotModified);
	    } 
        [Test]
        public async Task Should_Get_Id_From_Existing_Document()
        {
            var db = client.GetDatabase(baseDatabase);
            string id = "test_should_get_id";
            await db.CreateDocument("{\"_id\":\""+ id+"\"}");
            Document doc= await db.GetDocument(id);
            Assert.AreEqual(id, doc.Id);
        }

        [Test]
        public async Task Should_Populate_Items_When_IncludeDocs_Set_In_ViewOptions()
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
            await db.CreateDocument(doc._id, JsonConvert.SerializeObject(doc, Formatting.Indented, settings));

            var company = new Company();
            company.Name = "foo";
            await db.CreateDocument(company);

            // Without IncludeDocs
            var view = await db.View<Company>(viewName, designDoc);
            Assert.IsNull(view.Items.ToList()[0]);

            // With IncludeDocs
            ViewOptions options = new ViewOptions { IncludeDocs = true };
            view = await db.View<Company>(viewName, options, designDoc);
            Assert.AreEqual("foo", view.Items.ToList()[0].Name);
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
