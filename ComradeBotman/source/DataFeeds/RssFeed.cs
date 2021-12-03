using System;
using System.Linq;
using System.Xml;
using System.Threading.Tasks;

namespace ComradeBotman.DataFeeds
{
    sealed class RssFeed : WebDataFeed
    {
        private string channelTitle;

        private string lastUrlMessage;

        public RssFeed(Uri url, string channelTitle) : base(url) 
        {
            this.channelTitle = channelTitle;
            this.lastUrlMessage = null;
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

                if (string.Equals(this.lastUrlMessage, url))
                {
                    return null;
                }
                else
                {
                    this.lastUrlMessage = url;
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