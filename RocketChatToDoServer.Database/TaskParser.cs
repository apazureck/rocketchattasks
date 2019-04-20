using RocketChatToDoServer.Database.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
        private const string userGroup = "name";
        private const string taskGroup = "task";
        public string SeparateUsersAndTaskRegex { get => separateUsersAndTaskRegex != null ? separateUsersAndTaskRegex.ToString() : null; set => separateUsersAndTaskRegex = new Regex(value); }
        private Regex separateUsersAndTaskRegex = new Regex($@"(?:@(?<{userGroup}>\w+).*?)+:(?<{taskGroup}>.*)");

        public void ParseMessage(string message)
        {
            // get users via regex:

            var usermatch = separateUsersAndTaskRegex.Match(message);
            string taskmessage = usermatch.Groups[taskGroup].Value;
            DateTime? duedate = GetDueDate(taskmessage);
            foreach (System.Text.RegularExpressions.Capture username in usermatch.Groups[userGroup].Captures)
            {
                var user = GetUser(username.Value);
                CreateTask(user, taskmessage, duedate);
            }
        }

        private DateTime GetDueDate(object taskMessage)
        {
            return DateTime.Now;
        }

        /// <summary>
        /// Regular expression to parse a task
        /// </summary>
        public string ParseExpression { get; set; }

        public Func<string, User> GetUser { get; set; }
        public CreateTaskDelegate CreateTask { get; set; }
    }

    public delegate Task CreateTaskDelegate(User owner, string taskDescription, DateTime? dueDate);
}
