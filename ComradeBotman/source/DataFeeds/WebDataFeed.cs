using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ComradeBotman.DataFeeds
{
    abstract class WebDataFeed : IDataFeed, IDisposable
    {
        private Uri url;
        private HttpClient client;

        protected WebDataFeed(Uri url)
        {
            this.url = url;
            this.client = new HttpClient();            
        }

        protected string GetWebContent() => this.GetWebContentAsync().Result;

        protected Task<string> GetWebContentAsync() => this.client.GetStringAsync(this.url);

        public virtual string GetMessage() => this.GetMessageAsync().Result;

        public abstract Task<string> GetMessageAsync();

        public void Dispose()
        {
            this.Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool finalising)
        {
            this.client.Dispose();
        }
        
        ~WebDataFeed()
        {
            this.Dispose(true);
        }
    }
}