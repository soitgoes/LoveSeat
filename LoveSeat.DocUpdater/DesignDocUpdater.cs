using System.IO;
using LoveSeat.Repositories;
using Newtonsoft.Json.Linq;

namespace LoveSeat.DocUpdater
{
    public class DesignDocUpdater
    {
        private readonly CouchDatabase db;
        private readonly string filePath;

        public DesignDocUpdater(CouchDatabase db, string filePath)
        {
            this.db = db;
            this.filePath = filePath;
        }

        public void UpdateIfNecessary()
        {
            var repo = new CouchRepository<DesignDoc>(db);
            foreach (var file in Directory.GetFiles(this.filePath))
            {
                //get the hash of the file.  Compare against the hash on the design doc.  If they don't match then update.
                string designId = "_design/" + Path.GetFileNameWithoutExtension(file);
                string contents = File.ReadAllText(file);

                JObject obj = JObject.Parse(contents);
                string hash = Sha1Util.Sha1HashStringForUtf8String(contents);
                var doc = db.GetDocument<DesignDoc>(designId);
                if (doc == null )
                {
                   var newDoc = new DesignDoc
                        {
                            Id = designId,
                            Hash = hash,
                            Views = obj
                        };
                    repo.Save(newDoc);
                    return;
                }
                var designDoc = doc.Item;

                designDoc.Views = obj;
                designDoc.Hash = hash;
                repo.Save(designDoc);
       }
        }
    }
}
