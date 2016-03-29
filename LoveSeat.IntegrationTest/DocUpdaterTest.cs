using System;
using NUnit.Framework;
using LoveSeat.DocUpdater;

namespace LoveSeat.IntegrationTest
{
    [TestFixture]
    public class DocUpdaterTest
    {
        [Test]
        public void RunUpdate()
        {
            var db = new CouchDatabase(new Uri("http://localhost:5984/tmp"));
            var updater = new DesignDocUpdater(db, "couch_views");
            updater.UpdateIfNecessary();

        }
    }
}