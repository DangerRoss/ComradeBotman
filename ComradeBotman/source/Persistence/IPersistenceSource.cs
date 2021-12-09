using System;

namespace ComradeBotman.Persistence
{
    interface IPersistenceSource
    {
        void Load(PersistenceStore store);

        void Flush(PersistenceStore store);
    }
}
