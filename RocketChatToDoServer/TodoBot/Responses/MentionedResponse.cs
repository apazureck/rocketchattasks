using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rocket.Chat.Net.Bot;
using Rocket.Chat.Net.Bot.Interfaces;
using Rocket.Chat.Net.Bot.Models;
using RocketChatToDoServer.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Task = RocketChatToDoServer.Database.Models.Task;

namespace RocketChatToDoServer.TodoBot.Responses
{
    public class MentionedResponse : Response<NotifyUserMessageArgument>
    {
        private readonly ILogger logger;
        private readonly TaskContext context;

        private const string TASKTEMPLATENAME = "TaskTemplate";
        private const string DEFAULTTASKTEMPLATE = "**{{ID}}**: {{TaskDescription}}{{NotDefaultDate DueDate}} - Due: {{DueDate}}{{/NotDefaultDate}}";
        private const string DEFAULTTASKLISTTEMPLATE = "**Your open Tasks:**\n{{#each this}}{{> " + TASKTEMPLATENAME + "}}\n{{/each}}";

        private static Func<object, string> TaskListTemplate { get; set; }

        static MentionedResponse()
        {
            HandlebarsBlockHelper notDefaultDateHelper = (TextWriter output, HelperOptions options, dynamic context, object[] arguments) => {
                if (arguments.Length != 1)
                {
                    throw new HandlebarsException("{{NotDefaultDateHelper}} helper must have exactly one argument");
                }
                var arg = (DateTime)arguments[0];
                if (arg != default)
                {
                    options.Template(output, context);
                }
            };
            Handlebars.RegisterHelper("NotDefaultDate", notDefaultDateHelper);

            Handlebars.RegisterHelper("FormatDate", (writer, context, parameters) =>
            {
                var date = (DateTime)parameters[0];
                string format = parameters[1] as string;
                writer.WriteSafeString(date.ToString(format));
            });

            Handlebars.RegisterTemplate(TASKTEMPLATENAME, DEFAULTTASKTEMPLATE);
            TaskListTemplate = Handlebars.Compile(DEFAULTTASKLISTTEMPLATE);
        }

        public MentionedResponse(ILogger<MentionedResponse> logger, TaskContext context)
        {
            this.logger = logger;
            this.context = context;
        }
        protected override IMessageResponse RespondTo(NotifyUserMessageArgument input)
        {
            if (input.IsDirectMessage)
            {
                return RespondToDirectMessage(input);
            }
            return new BasicResponse("Sorry, I do not know what you mean.");
        }

        private IMessageResponse RespondToDirectMessage(NotifyUserMessageArgument input)
        {
            IMessageResponse response = null;
            if (input.Payload.Message.Msg.TrimStart().ToUpper().StartsWith("TASKLIST"))
            {
                response = RespondToTaskList(input);
            }
            else if (input.Payload.Message.Msg.TrimStart().ToUpper().StartsWith("SET TASK"))
            {
                response = RespondToSetTaskDone(input);
            }
            if(response != null)
            {
                response.RoomId = response.RoomId ?? input.Payload.RoomId;
                return response;
            }
            return new BasicResponse("Sorry, I did not understand your request. I understand the following actions:\n* **tasklist**: Display all tasks assigned to you\n* Set Task <taskId> to <open/done>: Set a specific task to open or done", input.Payload.RoomId);
        }

        private IMessageResponse RespondToSetTaskDone(NotifyUserMessageArgument input)
        {
            try
            {
                var rgx = new Regex(@"set task (?<taskId>\d+)(?: to)? (?<order>\w+)");
                var result = rgx.IsMatch(input.Text);
                var match = Regex.Match(input.Text, @"set task (?<taskId>\d+)(?: to)? (?<order>\w+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    int taskid = int.Parse(match.Groups["taskId"].Value);
                    Task task = context.Tasks.First(x => x.ID == taskid);
                    string setvalue = match.Groups["order"].Value;
                    switch (setvalue)
                    {
                        case "done":
                        case "finished":
                            task.Done = true;
                            break;
                        case "open":
                        case "reopen":
                        case "undone":
                            task.Done = false;
                            break;
                        default:
                            return new BasicResponse($"I don't know what to do with Task {task.ID}. Use either 'done' or 'open'.");
                    }
                    context.SaveChanges();
                    return new BasicResponse($"Sucessfully set Task {task.ID} {setvalue}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error setting task status. Input message: '{message}'", input.Text);
            }
            return null;
        }

        private IMessageResponse RespondToTaskList(NotifyUserMessageArgument input)
        {
            IEnumerable<Task> tl = GetTaskList(context, input.Title.Substring(1)).Where(x => !x.Done);
            string rs = tl.Select(t => t.ID + " " + t.TaskDescription + (t.DueDate != default ? "; DUE: " + t.DueDate : "")).Aggregate("", (a, b) => a + $"\n- " + b);
            return new BasicResponse(TaskListTemplate(tl), input.Payload.RoomId);
        }

        private ICollection<Task> GetTaskList(TaskContext context, string username) => context.Users.Include(u => u.Tasks).First(x => x.Name == username).Tasks;
    }
}
