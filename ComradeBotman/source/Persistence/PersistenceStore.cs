using System;
using System.Collections.Generic;

namespace ComradeBotman.Persistence
{
    sealed class PersistenceStore
    {
        private Dictionary<string, string> store;
        
        public PersistenceStore()
        {
            this.store = new Dictionary<string, string>();
        }

        public void Load(IPersistenceSource source) => source.Load(this);
               
        public void Flush(IPersistenceSource source) => source.Flush(this);

        public Dictionary<string, string> GetKeyValues()
        {
            lock(this)
            {
                return new Dictionary<string, string>(this.store);
            }            
        }

        public void SetKeyValues(Dictionary<string, string> keyValues)
        {
            if(keyValues == null)
            {
                throw new ArgumentNullException();
            }

            lock(this)
            {
                this.store = keyValues;
            }            
        }

        public string GetKeyValue(string key)
        {
            lock(this)
            {
                if (this.store.TryGetValue(key, out var value))
                {
                    return value;
                }
                else
                {
                    return null;
                }
            }            
        }

        public void SetKeyValue(string key, string value)
        {
            lock(this)
            {
                if (!this.store.TryAdd(key, value))
                {
                    this.store[key] = value;                    
                }
            }            
        }
    }
}