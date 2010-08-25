using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
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
			client = new CouchClient(host , port, username, password);
			if (!client.HasDatabase(baseDatabase))
			{
				client.CreateDatabase(baseDatabase);				
			}
		}	

		[Test]
		public void CanTriggerReplication()
		{
			var obj  = client.TriggerReplication("http://Professor:Farnsworth@"+ host+":5984/" +replicateDatabase, baseDatabase);
			Assert.IsTrue(obj != null);
		}
		[Test]
		public void CanCreateDocumentFromString()
		{
			string obj = @"{""test"": ""prop""}";
			var db = client.GetDatabase(baseDatabase);
			var result = db.CreateDocument("fdas", obj);
			Assert.IsNotNull(db.GetDocument("fdas"));
		}
		[Test]
		public void CanDeleteDocument()
		{
			var db = client.GetDatabase(baseDatabase);
			db.CreateDocument("asdf", "{}");
			var doc = db.GetDocument("asdf");
			var result = 	db.DeleteDocument(doc.Id(), doc.Rev());
			Assert.IsNull(db.GetDocument("asdf"));
		}


		[TestFixtureTearDown]
		public void TearDown()
		{
			//delete the test database
			if (client.HasDatabase(baseDatabase))
			{
				client.DeleteDatabase(baseDatabase);
			}
			
		}

	}
}
