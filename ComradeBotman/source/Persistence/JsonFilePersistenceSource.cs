using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace ComradeBotman.Persistence
{
    sealed class JsonFilePersistenceSource : IPersistenceSource
    {
        private readonly FileInfo file;

        public JsonFilePersistenceSource(FileInfo file)
        {
            this.file = file;
        }

        public void Flush(PersistenceStore store)
        {
            var kvps = store.GetKeyValues();

            using var fs = new FileStream(this.file.FullName, FileMode.OpenOrCreate, FileAccess.Write);
            using var writer = new Utf8JsonWriter(fs);

            JsonSerializer.Serialize(writer, new Model() { keyvalues = kvps });
        }

        public void Load(PersistenceStore store)
        {
            if(this.file.Exists)
            {
                byte[] data;

                using (var fs = new FileStream(this.file.FullName, FileMode.Open, FileAccess.Read))
                {
                    data = new byte[fs.Length];
                    fs.Read(data);
                }

                var model = JsonSerializer.Deserialize<Model>(data);

                store.SetKeyValues(model.keyvalues);
            }
        }

        private class Model
        {
            public Dictionary<string, string> keyvalues { get; set; }
        }
    }
}