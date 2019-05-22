using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rocket.Chat.Net.Bot;
using Rocket.Chat.Net.Interfaces;
using Rocket.Chat.Net.Models.LoginOptions;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.TodoBot.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RocketChatToDoServer.TodoBot
{
    public class BotService
    {
        private readonly ILogger<BotService> logger;

        private readonly List<BotConfiguration> botConfigurations = new List<BotConfiguration>();
        private readonly List<RcDiBot> bots = new List<RcDiBot>();
        private readonly List<ILoginOption> loginOptions = new List<ILoginOption>();

        public BotService(ILogger<BotService> logger, IConfiguration configuration, IServiceCollection services)
        {
            configuration.GetSection("bots").Bind(botConfigurations);
            this.logger = logger;
            foreach(BotConfiguration botconfig in botConfigurations)
            {
                loginOptions.Add(new LdapLoginOption
                {
                    Username = botconfig.Username,
                    Password = botconfig.Password
                });

                // SetUp Bot
                bots.Add(new RcDiBot(botconfig.RocketServerUrl, botconfig.UseSsl, services, logger, botconfig.ResponseUrl));
            }
        }

        public async Task SendMessageToUser(int userId, string message)
        {
            await bots.First().SendMessageAsync(message, userId);
        }

        public async void LoginAsync()
        {
            for(int i = 0; i < bots.Count; i++)
            {
                RcDiBot bot = bots[i];

                try
                {
                    ILoginOption loginOption = loginOptions[i];

                    await bot.ConnectAsync();
                    await bot.LoginAsync(loginOption);
                    await bot.SubscribeAsync<MentionedResponse>(Stream.NotifyUser_Notifications, (provider, b) =>
                    {
                        TaskParser.TaskParserService taskParser = provider.GetService<TaskParser.TaskParserService>();
                        taskParser.Username = b.Driver.Username;
                        return new MentionedResponse(provider.GetService<ILogger<MentionedResponse>>(), provider.GetService<TaskContext>(), taskParser, bot.ResponseUrl);
                    }, false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Could not login bot " + bot.Driver.Username);
                }
            }            
        }
    }

    public class BotConfiguration
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string RocketServerUrl { get; set; }
        public bool UseSsl { get; set; }
        /// <summary>
        /// This is the base url to the services on the server endpoint of the todo server.
        /// </summary>
        public string ResponseUrl { get; set; }
    }
}
