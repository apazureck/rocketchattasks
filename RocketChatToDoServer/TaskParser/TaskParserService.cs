using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rocket.Chat.Net.Models;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.TodoBot;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RocketChatToDoServer.TaskParser
{
    public class TaskParserService
    {
        private Database.TaskParser taskParser;
        private readonly ILogger<TaskParserService> logger;
        private readonly TaskContext context;
        private readonly BotConfiguration botConfiguration = new BotConfiguration();

        public TaskParserService(ILogger<TaskParserService> logger, TaskContext context, IConfiguration config)
        {
            config.GetSection("bot").Bind(botConfiguration);
            this.logger = logger;
            this.context = context;
            taskParser = new Database.TaskParser(logger, (username, log) =>
            {
                return context.Users.First(x => x.Name == username);
            }, (owner, taskDescription, log, dueDate) => {
                Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Database.Models.Task> task = context.Tasks.Add(new Database.Models.Task()
                {
                    CreationDate = DateTime.Now,
                    DueDate = dueDate ?? default,
                    TaskDescription = taskDescription,
                    UserID = owner.ID
                });
                context.SaveChanges();
                return task.Entity;
            });
        }

        public Database.Models.Task GetTask(string inputMessage)
        {
            return taskParser.ParseMessage(inputMessage.Replace("@" + botConfiguration.Username, ""), logger);
        }
    }
}
