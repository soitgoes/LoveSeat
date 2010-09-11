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
		}
		[TestFixtureTearDown]
		public void TearDown()
		{
			//delete the test database
			if (client.HasDatabase(baseDatabase))
			{
				client.DeleteDatabase(baseDatabase);
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

		

	}
}
