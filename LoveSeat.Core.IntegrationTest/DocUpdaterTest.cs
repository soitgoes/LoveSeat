using LoveSeat.Core.DocUpdater;
using System.IO;
using Xunit;

namespace LoveSeat.Core.IntegrationTest
{
    public class DocUpdaterTest
    {
        [Fact]
        public void RunUpdate()
        {
            var client = new CouchClient("admin", "pass");
            if(!client.HasDatabase("tmp"))
                client.CreateDatabase("tmp");

            var db = client.GetDatabase("tmp");
            var updater = new DesignDocUpdater(db, $"{Directory.GetCurrentDirectory()}/couch_views");
            updater.UpdateIfNecessary();
        }
    }
}