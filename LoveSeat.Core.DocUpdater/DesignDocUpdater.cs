using System.IO;
using Newtonsoft.Json.Linq;
using LoveSeat.Core.Repositories;

namespace LoveSeat.Core.DocUpdater
{
    public class DesignDocUpdater
    {
        private readonly CouchDatabase _db;
        private readonly string _directoryPath;

        public DesignDocUpdater(CouchDatabase db, string directoryPath)
        {
            _db = db;
            _directoryPath = directoryPath;
        }

        public void UpdateIfNecessary()
        {
            var repo = new CouchRepository<DesignDoc>(_db);

            foreach (var file in Directory.GetFiles(_directoryPath, "*.json"))
            {
                // Get the hash of the file.  
                // Compare against the hash on the design doc.
                // If they don't match then update.
                string designId = "_design/" + Path.GetFileNameWithoutExtension(file);
                string contents = File.ReadAllText(file);

                JObject obj = JObject.Parse(contents);
                string hash = Sha1Util.Sha1HashStringForUtf8String(contents);
                var doc = _db.GetDocument<DesignDoc>(designId);
                if (doc == null)
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
