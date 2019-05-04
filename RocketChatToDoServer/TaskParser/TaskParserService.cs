using Microsoft.Extensions.Logging;
using RocketChatToDoServer.Database;
using System;
using System.Linq;

namespace RocketChatToDoServer.TaskParser
{
    public class TaskParserService
    {
        private Database.TaskParser taskParser;
        private readonly ILogger<TaskParserService> logger;
        private readonly TaskContext context;

        public string Username { get; set; }

        public TaskParserService(ILogger<TaskParserService> logger, TaskContext context, string userName = null)
        {
            this.logger = logger;
            this.context = context;
            Username = userName;
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
            return taskParser.ParseMessage(inputMessage.Replace("@" + Username, ""), logger);
        }
    }
}
