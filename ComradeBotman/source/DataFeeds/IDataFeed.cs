using System;
using System.Threading.Tasks;

namespace ComradeBotman.DataFeeds
{
    interface IDataFeed : IDisposable
    {
        string GetMessage();

        Task<string> GetMessageAsync();
    }
}