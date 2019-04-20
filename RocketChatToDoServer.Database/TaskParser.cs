using RocketChatToDoServer.Database.Models;
using System;
using System.Collections.Generic;

namespace RocketChatToDoServer.Database
{
    public class TaskParser
    {
        public TaskParser()
        {
            GetUser = (string userName) =>
            {
                return new User()
                {
                    ID = 1,
                    Name = userName,
                    Tasks = new List<Task>()
                };
            };
            CreateTask = (User owner, string taskDescription, DateTime? dueDate) =>
            {
                return new Task()
                {
                    CreationDate = DateTime.Now,
                    DueDate = dueDate ?? default(DateTime),
                    ID = 1,
                    TaskDescription = taskDescription,
                    User = owner,
                    UserID = owner.ID
                };
            };
        }

        public string SeparateÛsersAndTaskRegex { get; set; } = @"(?:@(?<name>\w +).*?)+:(?<task>.*)";

        /// <summary>
        /// Regular expression to parse a task
        /// </summary>
        public string ParseExpression { get; set; }

        public Func<string, User> GetUser { get; set; }
        public CreateTaskDelegate CreateTask { get; set; }
    }

    public delegate Task CreateTaskDelegate(User owner, string taskDescription, DateTime? dueDate);
}
