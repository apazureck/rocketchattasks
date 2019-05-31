using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Rocket.Chat.Net.Bot;
using Rocket.Chat.Net.Bot.Interfaces;
using Rocket.Chat.Net.Bot.Models;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RocketChatToDoServer.TodoBot
{
    public class TaskListBuilder
    {
        static TaskListBuilder()
        {
            Handlebars.RegisterTemplate(TASKTEMPLATENAME, DEFAULTTASKTEMPLATE);
            TaskListTemplate = Handlebars.Compile(DEFAULTTASKLISTTEMPLATE);
        }

        private const string TASKTEMPLATENAME = "TaskTemplate";
        private const string DEFAULTTASKTEMPLATE = "[\u2610]({{DoneLink}}) **[Task {{Task.ID}}]({{TaskLink}})**: {{Task.Title}}{{NotDefaultDate Task.DueDate}} - Due: {{Task.DueDate}}{{/NotDefaultDate}}";
        private const string DEFAULTTASKLISTTEMPLATE = "**[Your open Tasks]({{userTaskLink}}):**\n{{#each tasks}}{{> " + TASKTEMPLATENAME + "}}\n{{/each}}";

        private static Func<object, string> TaskListTemplate { get; set; }
        public string ResponseUrl { get; }

        private readonly TaskContext context;

        public TaskListBuilder(TaskContext context, string responseUrl = null)
        {
            this.context = context;
            ResponseUrl = responseUrl;
        }

        /// <summary>
        /// Gets the tasklist for the user, using 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="titleText"></param>
        /// <returns></returns>
        public IMessageResponse GetMessage(string username, string titleText)
        {
            User user = context.Users.First(u => u.Name == username);
            IEnumerable<Task> tl = GetTaskList(context, user).Where(x => !x.Done).ToList();
            if (tl.Count() < 1)
                throw new InvalidOperationException("Tasklist is empty");

            return new BasicResponse(titleText + "\n" + TaskListTemplate(new
            {
                tasks = tl.Select(x => new
                {
                    Task = x,
                    User = user,
                    DoneLink = CreateDoneUrl(x, user),
                    TaskLink = this.CreateTaskUrl(x),
                    CheckBoxImageLink = this.CreateCheckBoxUrl()
                }),
                userTaskLink = ResponseUrl + $"/users/{user.ID}"
            }));
        }

        private ICollection<Task> GetTaskList(TaskContext context, User user)
        {
            IQueryable<UserTaskMap> utmaps = context.UserTaskMaps.Include(utm => utm.Task).Where(utm => utm.UserID == user.ID);
            List<Task> tl = utmaps.Select(utm => utm.Task).ToList();
            return tl;
        }

        private string CreateTaskUrl(Task t) => $"{ResponseUrl}/tasks/{t.ID}";

        private string CreateDoneUrl(Task x, User user) => ResponseUrl + $"/users/{user.ID}/setDone/{x.ID}";

        private string CreateCheckBoxUrl() => $"{ResponseUrl}/assets/checkbox_unchecked.png";
    }
}
