using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace LoveSeat.IntegrationTest
{
	[TestFixture]
	public class CouchClientTest
	{
		private CouchClient client;

		[SetUp]
		public void Setup()
		{
			client = new CouchClient("localhost", 5984, "bubba", "password");
		}

		[Test]
		public void CanGetSessionCookie()
		{
			var cookie = client.GetSession();
			Assert.IsNotNull(cookie);
		}

		[Test]
		public void CanTriggerReplication()
		{
			var obj  = client.TriggerReplication("http://bubba:password@remotedb.com:5984/remotedb", "local");
			Assert.IsTrue(obj != null);
		}
	}
}
