using System.Configuration;
using System.IO;
using System.Linq;
using LoveSeat.Support;
using NUnit.Framework;

namespace LoveSeat.IntegrationTest
{
	[TestFixture]
	public class CouchClientTest
	{
		private CouchClient client;
		private const string baseDatabase = "love-seat-test-base";
        private const string replicateDatabase = "love-seat-test-repli";

		private readonly string host = ConfigurationManager.AppSettings["Host"].ToString();
		private readonly int port = int.Parse(ConfigurationManager.AppSettings["Port"].ToString());
		private readonly string username = ConfigurationManager.AppSettings["UserName"].ToString();
		private readonly string password = ConfigurationManager.AppSettings["Password"].ToString();

		[TestFixtureSetUp]
		public void Setup()
		{
			client = new CouchClient(host, port, username, password);
			if (!client.HasDatabase(baseDatabase))
			{
				client.CreateDatabase(baseDatabase);
			}
            if (!client.HasDatabase(replicateDatabase))
            {
                client.CreateDatabase(replicateDatabase);
            }
		}
		[TestFixtureTearDown]
		public void TearDown()
		{
			//delete the test database
			if (client.HasDatabase(baseDatabase))
			{
				client.DeleteDatabase(baseDatabase);
			}
            if (client.HasDatabase(replicateDatabase))
            {
                client.DeleteDatabase(replicateDatabase);
            }
			if (client.HasUser("Leela"))
			{
				client.DeleteAdminUser("Leela");	
			}
		}

		[Test]
		public void Should_Trigger_Replication()
		{
			var obj  = client.TriggerReplication("http://Professor:Farnsworth@"+ host+":5984/" +replicateDatabase, baseDatabase);
			Assert.IsTrue(obj != null);
		}
		[Test]
		public void Should_Create_Document_From_String()
		{
			string obj = @"{""test"": ""prop""}";
			var db = client.GetDatabase(baseDatabase);
			var result = db.CreateDocument("fdas", obj);
			Assert.IsNotNull(db.GetDocument("fdas"));
		}
        [Test]
        public void Should_Save_Existing_Document()
        {
            string obj = @"{""test"": ""prop""}";
            var db = client.GetDatabase(baseDatabase);
            var result = db.CreateDocument("fdas", obj);
            var doc = db.GetDocument("fdas");
            doc["test"] = "newprop";
            var newresult = db.SaveDocument(doc);
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
	    [ExpectedException("LoveSeat.NotModifiedException")]
	    public void Should_Throw_Not_Modified_Exception_If_Etag_Matches()
	    {
            var db = client.GetDatabase(baseDatabase);
            db.CreateDocument(@"{""_id"":""test_eTag_exception""}");
            ViewResult result = db.GetAllDocuments();
	        db.GetAllDocuments(new ViewOptions {Etag = result.Etag});
	    }
	}
}
