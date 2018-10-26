using NUnit.Framework;
using System;
using System.Configuration;

namespace LoveSeat.IntegrationTest
{
    [TestFixture]
    public class CouchDatabaseTest
    {
        private CouchDatabase db;
        private CouchClient client;
        private const string baseDatabase = "love-seat-test-base";

        private readonly string host = ConfigurationManager.AppSettings["Host"];
        private readonly int port = int.Parse(ConfigurationManager.AppSettings["Port"]);
        private readonly string username = ConfigurationManager.AppSettings["UserName"];
        private readonly string password = ConfigurationManager.AppSettings["Password"];

        [SetUp]
        public void Setup()
        {
            client = new CouchClient(host, port, username, password, false, AuthenticationType.Cookie);
            if (!client.HasDatabase(baseDatabase))
            {
                client.CreateDatabase(baseDatabase);
            }

            var uriBuilder = new UriBuilder("http", host, port, baseDatabase);
            db = new CouchDatabase(uriBuilder.Uri);
        }

        [TearDown]
        public void TearDown()
        {
            //delete the test database
            if (client.HasDatabase(baseDatabase))
            {
                client.DeleteDatabase(baseDatabase);
            }
        }

        [Test]
        public void Should_Save_Multiple_Documents()
        {
            var pets = new Documents();
            pets.Values.Add(new Document(@"{ ""name"": ""Santa's Little Helper"" }"));
            pets.Values.Add(new Document(@"{ ""name"": ""Snowball V"" }"));
            pets.Values.Add(new Document(@"{ ""name"": ""Strangles"" }"));

            db.SaveDocuments(pets, true);
        }
    }
}
