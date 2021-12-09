using System;
using System.Linq;
using System.Xml;
using System.Threading.Tasks;

using ComradeBotman.Persistence;

namespace ComradeBotman.DataFeeds
{
    sealed class RssFeed : WebDataFeed
    {
        private string channelTitle;
        private string lastUrlKey;

        private PersistenceStore store;

        public RssFeed(Uri url, string channelTitle, PersistenceStore store, string lastUrlKey) : base(url) 
        {
            this.channelTitle = channelTitle;
            this.lastUrlKey = lastUrlKey;
            this.store = store;
        }

        public override Task<string> GetMessageAsync() => CreateMessage();

        private async Task<string> CreateMessage()
        {
            try
            {
                var xmlfeed = await this.GetWebContentAsync();

                if (string.IsNullOrWhiteSpace(xmlfeed))
                {
                    return null;
                }

                var xmldoc = new XmlDocument();
                xmldoc.LoadXml(xmlfeed.TrimStart());

                var channel = (from XmlNode ch in xmldoc.GetElementsByTagName("channel") where ch["title"].InnerText.Equals(channelTitle) select ch).First();

                var items =
                from XmlNode its
                in ((XmlElement)channel).GetElementsByTagName("item")
                orderby DateTime.Parse(its["pubDate"].InnerText) descending
                select its;

                var url = items.First()["link"].InnerText;

                if (string.Equals(this.store.GetKeyValue(this.lastUrlKey), url))
                {
                    return null;
                }
                else
                {
                    this.store.SetKeyValue(this.lastUrlKey, url);
                    return url;
                }
            }
            catch(Exception e)
            {
                Console.Out.WriteLine(e);
                return null;
            }            
        }
    }
}