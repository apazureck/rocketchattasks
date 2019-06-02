using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

            return new TasklistMessageResponse(titleText, ResponseUrl, user, tl);
        }

        private ICollection<Task> GetTaskList(TaskContext context, User user)
        {
            IQueryable<UserTaskMap> utmaps = context.UserTaskMaps.Include(utm => utm.Task).Where(utm => utm.UserID == user.ID);
            var tl = utmaps.Select(utm => utm.Task).ToList();
            return tl;
        }

        public IMessageResponse UpdateMessage(IMessageResponse taskListResponse)
        {
            if (!(taskListResponse is TasklistMessageResponse response))
                throw new ArgumentException("Response has to be of type TasklistMessageResponse", nameof(taskListResponse));

            var newTaskList = response.GetTasks().Select(x => context.Tasks.FirstOrDefault(t => t.ID == x.ID));
            return new TasklistMessageResponse(response.GetTitle(), response.ResponseUrl, response.GetUser(), newTaskList);
        }
    }

    public class TasklistMessageResponse : BasicResponse
    {
        private readonly string title;
        private readonly User user;
        private static Func<object, string> TaskListTemplate { get; set; }
        private readonly IEnumerable<Task> tasks;
        public string ResponseUrl { get; }

        static TasklistMessageResponse()
        {
            Handlebars.RegisterTemplate(TASKTEMPLATENAME, DEFAULTTASKTEMPLATE);
            TaskListTemplate = Handlebars.Compile(DEFAULTTASKLISTTEMPLATE);
        }

        private const string TASKTEMPLATENAME = "TaskTemplate";
        private const string DEFAULTTASKTEMPLATE = "{{#if Task.Done}}[\u2611]({{UndoneLink}}){{else}}[\u2610]({{DoneLink}}){{/if}} **[Task {{Task.ID}}]({{TaskLink}})**: {{Task.Title}}{{NotDefaultDate Task.DueDate}} - Due: {{Task.DueDate}}{{/NotDefaultDate}}";
        private const string DEFAULTTASKLISTTEMPLATE = "**[Your open Tasks]({{userTaskLink}}):**\n{{#each tasks}}{{> " + TASKTEMPLATENAME + "}}\n{{/each}}";

        private static string CreateTaskUrl(Task t, string responseUrl) => $"{responseUrl}/tasks/{t.ID}";
        private static string CreateDoneUrl(Task x, User user, string responseUrl) => responseUrl + $"/users/{user.ID}/setDone/{x.ID}";
        private static string CreateUndoneUrl(Task x, User user, string responseUrl) => responseUrl + $"/users/{user.ID}/setUndone/{x.ID}";


        public TasklistMessageResponse(string title, string responseUrl, User user, IEnumerable<Task> tasks) : base(title + "\n" + TaskListTemplate(new
        {
            tasks = tasks.Select(x => new
            {
                Task = x,
                User = user,
                DoneLink = CreateDoneUrl(x, user, responseUrl),
                UndoneLink = CreateUndoneUrl(x, user, responseUrl),
                TaskLink = CreateTaskUrl(x, responseUrl)
            }),
            userTaskLink = responseUrl + $"/users/{user.ID}"
        }))
        {
            this.title = title;
            ResponseUrl = responseUrl;
            this.user = user;
            this.tasks = tasks;
        }

        public User GetUser() => user;

        public IEnumerable<Task> GetTasks() => tasks;

        public string GetTitle() => title;
    }
}
