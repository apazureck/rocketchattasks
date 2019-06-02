using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rocket.Chat.Net.Bot;
using Rocket.Chat.Net.Driver;
using Rocket.Chat.Net.Interfaces;
using Rocket.Chat.Net.Models;
using Rocket.Chat.Net.Models.LoginOptions;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.Database.Models;
using RocketChatToDoServer.TodoBot.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using Tasks = System.Threading.Tasks;

namespace RocketChatToDoServer.TodoBot
{
    public class BotService : IPrivateMessenger
    {
        private readonly ILogger<BotService> logger;
        private readonly RocketChatCache cache;
        private readonly List<BotConfiguration> botConfigurations = new List<BotConfiguration>();
        private readonly List<RcDiBot> bots = new List<RcDiBot>();
        private readonly List<ILoginOption> loginOptions = new List<ILoginOption>();

        public BotService(ILogger<BotService> logger, IConfiguration configuration, IServiceCollection services, RocketChatCache cache)
        {
            configuration.GetSection("bots").Bind(botConfigurations);
            this.logger = logger;
            this.cache = cache;
            foreach (BotConfiguration botconfig in botConfigurations)
            {
                loginOptions.Add(new LdapLoginOption
                {
                    Username = botconfig.Username,
                    Password = botconfig.Password
                });

                // SetUp Bot
                bots.Add(new RcDiBot(botconfig.RocketServerUrl, botconfig.UseSsl, services, cache, logger, botconfig.ResponseUrl));
            }
        }

        internal async Tasks.Task<bool> CheckUserLogin(string userName, string password)
        {
            var config = botConfigurations.First();
            var driver = new RocketChatDriver(config.RocketServerUrl, config.UseSsl, logger);
            return await driver.CheckLogin(userName, password);
        }

        public async Tasks.Task SendMessageToUser(int userId, string message)
        {
            await bots.First().SendMessageAsync(message, userId);
        }

        public async Tasks.Task LoginAsync()
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
                        return new MentionedResponse(provider.GetService<ILogger<MentionedResponse>>(), provider.GetService<TaskContext>(), taskParser, this, provider.GetService<RocketChatCache>(), bot.ResponseUrl);
                    }, false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Could not login bot " + bot.Driver.Username);
                }
            }            
        }

        public async Tasks.Task UpdateTaskList(int iD, Task taskToSetToDone)
        {
            await bots.First().UpdateLastTaskList(iD, taskToSetToDone);
        }

        public async System.Threading.Tasks.Task SendReminders()
        {
            foreach (var bot in bots)
            {
                await bot.SendReminders();
            }
        }

        internal async Tasks.Task<List<FullUser>> GetUserList()
        {
            return await bots[0].Driver.GetUserList();
        }

        public async Tasks.Task SendAssigneeMessage(Database.Models.User user, Database.Models.Task task, Database.Models.User initiator)
        {
            await SendMessageToUser(user.ID, $"You have ben assigned to **Task {task.ID}**: {task.Title}.");
        }

        public async Tasks.Task SendUnAssignedMessage(Database.Models.User user, Database.Models.Task task, Database.Models.User initiator)
        {
            await SendMessageToUser(user.ID, $"Your assignment to **Task {task.ID}**: {task.Title} has been removed.");
        }

        public async Tasks.Task SendTaskUpdatedMessage(Task task, Database.Models.User initiator)
        {
            var sendusers = task.Assignees.Select(x => x.User).ToList();
            if (task.Assignees.FirstOrDefault(x => task.InitiatorID == x.UserID) == null)
                sendusers.Add(task.Initiator);

            foreach (Database.Models.User user in sendusers)
                await SendMessageToUser(user.ID, $"Task {task.ID} has been updated");
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
