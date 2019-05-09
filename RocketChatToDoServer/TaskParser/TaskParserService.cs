using Microsoft.Extensions.Logging;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.Database.Models;
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
                var user = context.Users.FirstOrDefault(x => x.Name == username);
                if(user == null)
                {
                    context.Add(new User
                    {
                        Name = username
                    });
                    context.SaveChanges();
                    user = context.Users.First(x => x.Name == username);
                }
                return user;
            }, (owner, assignees, taskDescription, log, dueDate) => {

                Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Database.Models.Task> task = context.Tasks.Add(new Database.Models.Task()
                {
                    CreationDate = DateTime.Now,
                    DueDate = dueDate ?? default,
                    Title = taskDescription,
                    InitiatorID = owner.ID
                });
                context.SaveChanges();
                context.AddRange(assignees.Select(a => new UserTaskMap
                {
                    TaskID = task.Entity.ID,
                    UserID = a.ID
                }));
                context.SaveChanges();
                return task.Entity;
            });
        }

        public Database.Models.Task GetTask(string initiator, string inputMessage)
        {
            return taskParser.ParseMessage(initiator, inputMessage.Replace("@" + Username, ""), logger);
        }
    }
}
