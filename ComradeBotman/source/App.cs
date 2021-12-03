using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ComradeBotman.DataFeeds;

using Discord;
using Discord.WebSocket;

namespace ComradeBotman
{
    sealed class App : IDisposable
    {
        private DiscordSocketClient client;

        private IDataFeed[] dataFeeds;

        private IEnumerable<SocketGuild> servers;

        private bool ready;

        public App()
        {
            this.ready = false;
            this.servers = Enumerable.Empty<SocketGuild>();
            this.client = new DiscordSocketClient();

            this.client.Log += this.Client_Log;
            this.client.Ready += this.Client_Ready;

            this.dataFeeds = new IDataFeed[]
            {
                new RssFeed(new Uri("https://thehardtimes.net/feed"), "The Hard Times"),
                new RssFeed(new Uri("https://hard-money.net/feed"), "Hard Money")
            };
        }

        private Task Client_Ready()
        {
            this.servers = this.client.Guilds;
            this.ready = true;
            return Task.CompletedTask;
        }

        private Task Client_Log(LogMessage arg)
        {
            Console.Out.WriteLine(arg.Message);
            return Task.CompletedTask;
        }

        public async Task RunAsync(string logintoken)
        {            
            await this.Login(logintoken);

            await this.client.StartAsync();

            await Task.Run(() => SpinWait.SpinUntil(() => this.ready, TimeSpan.FromSeconds(30)));

            if(!this.ready)
            {
                Console.Out.WriteLine("Failed to retrieve discord servers");
                return;
            }

            while(true) // (╯°□°）╯︵ ┻━┻
            {
                Console.Out.WriteLine("Checking data feeds");

                await ProcessDataFeeds();

                await Task.Delay(TimeSpan.FromMinutes(60));
            }            
        }

        private async Task ProcessDataFeeds()
        {
            var feeds = from IDataFeed dataFeed in this.dataFeeds select dataFeed.GetMessageAsync();
            var processed = this.dataFeeds.Length;
            
            foreach(var feedTask in feeds)
            {
                try
                {
                    var msg = await feedTask;

                    if (!string.IsNullOrEmpty(msg))
                    {
                        await Task.WhenAll(from server in this.servers select server.SystemChannel.SendMessageAsync(msg));
                    }
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine(e);
                }
            }
        }


        private async Task Login(string token)
        {
            await client.LoginAsync(TokenType.Bot, token);

            if (client.LoginState != LoginState.LoggedIn)
            {
                throw new ApplicationException("Login fail");
            }
        }

        public void Dispose()
        {
            this.Dispose(false);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool finalising)
        {
            Array.ForEach(this.dataFeeds, (feed) => feed.Dispose());
            this.client.Dispose();
        }

        ~App()
        {
            Dispose(true);
        }

        static void Main(string[] args)
        {
            var loginToken = GetLoginToken();

            if(string.IsNullOrWhiteSpace(loginToken))
            {
                Console.WriteLine("Please enter bot login token...");
                loginToken = Console.ReadLine();

                if(string.IsNullOrWhiteSpace(loginToken))
                {
                    Console.WriteLine("no login token provided, closing");
                    return;
                }
                Console.Clear();
            }

            using var app = new App();

            try
            {
                var runtask = app.RunAsync(loginToken);
                runtask.Wait();
            }
            catch(Exception e)
            {
                Console.Out.WriteLine("Unhandled Exception");
                Console.Out.WriteLine(e);
            }            
        }

        private static string GetLoginToken()
        {
            const string variable = "LoginToken";

            var args = Environment.GetCommandLineArgs();

            for(int i = 0; i < args.Length; i++)
            {
                if(args[i].Equals(variable))
                {
                    if(++i < args.Length)
                    {
                        return args[i];
                    }
                }
            }

            return Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Process);
        }        
    }
}