using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rocket.Chat.Net.Bot;
using Rocket.Chat.Net.Interfaces;
using Rocket.Chat.Net.Models.LoginOptions;
using RocketChatToDoServer.TodoBot.Responses;
using System;
using System.Threading.Tasks;

namespace RocketChatToDoServer.TodoBot
{
    public class Bot
    {
        private readonly ILogger<Bot> logger;
        private readonly BotConfiguration botConfiguration = new BotConfiguration();
        private readonly RocketChatBot bot;
        private readonly ILoginOption loginOption;

        public Bot(ILogger<Bot> logger, IConfiguration configuration)
        {
            configuration.GetSection("bot").Bind(botConfiguration);
            this.logger = logger;

            loginOption = new LdapLoginOption
            {
                Username = botConfiguration.Username,
                Password = botConfiguration.Password
            };

            // SetUp Bot
            bot = new RocketChatBot(botConfiguration.RocketServerUrl, botConfiguration.UseSsl, logger);
            bot.AddResponse(new TaskListResponse(logger));
        }

        public async Task Login()
        {
            await bot.LoginAsync(loginOption);
        }

        public async void LoginAsync()
        {
            await bot.LoginAsync(loginOption);
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
