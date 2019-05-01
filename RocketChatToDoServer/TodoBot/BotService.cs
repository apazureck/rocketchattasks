using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rocket.Chat.Net.Bot;
using Rocket.Chat.Net.Interfaces;
using Rocket.Chat.Net.Models.LoginOptions;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.TodoBot.Responses;
using System;
using System.Threading.Tasks;

namespace RocketChatToDoServer.TodoBot
{
    public class BotService
    {
        private readonly ILogger<BotService> logger;

        private readonly BotConfiguration botConfiguration = new BotConfiguration();
        private readonly RcDiBot bot;
        private readonly ILoginOption loginOption;

        public BotService(ILogger<BotService> logger, IConfiguration configuration, IServiceCollection services)
        {
            configuration.GetSection("bot").Bind(botConfiguration);
            this.logger = logger;
            loginOption = new LdapLoginOption
            {
                Username = botConfiguration.Username,
                Password = botConfiguration.Password
            };

            // SetUp Bot
            bot = new RcDiBot(botConfiguration.RocketServerUrl, botConfiguration.UseSsl, services, logger);
        }

        public async void LoginAsync()
        {
            await bot.ConnectAsync();
            await bot.LoginAsync(loginOption);
            await bot.SubscribeAsync<MentionedResponse>(Stream.NotifyUser_Notifications, false);
            
        }
    }

    public class BotConfiguration
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string RocketServerUrl { get; set; }
        public bool UseSsl { get; set; }
    }
}
