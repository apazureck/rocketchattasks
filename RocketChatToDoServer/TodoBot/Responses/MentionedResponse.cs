using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rocket.Chat.Net.Bot;
using Rocket.Chat.Net.Bot.Interfaces;
using Rocket.Chat.Net.Bot.Models;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.Database.Models;
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
        private readonly TaskParser.TaskParserService parserService;
        private const string TASKTEMPLATENAME = "TaskTemplate";
        private const string DEFAULTTASKTEMPLATE = "**{{Task.ID}}**: {{Task.Title}}{{NotDefaultDate Task.DueDate}} - Due: {{Task.DueDate}}{{/NotDefaultDate}} ([Done]({{DoneLink}}))";
        private const string DEFAULTTASKLISTTEMPLATE = "**Your open Tasks:**\n{{#each this}}{{> " + TASKTEMPLATENAME + "}}\n{{/each}}";

        private static Func<object, string> TaskListTemplate { get; set; }
        public string ResponseUrl { get; }

        static MentionedResponse()
        {
            void notDefaultDateHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
            {
                if (arguments.Length != 1)
                {
                    throw new HandlebarsException("{{NotDefaultDateHelper}} helper must have exactly one argument");
                }
                var arg = (DateTime)arguments[0];
                if (arg != default)
                {
                    options.Template(output, context);
                } else
                {
                    options.Inverse(output, context);
                }
            }
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

        public MentionedResponse(ILogger<MentionedResponse> logger, TaskContext context, TaskParser.TaskParserService parserService, string responseUrl = null)
        {
            this.logger = logger;
            this.context = context;
            this.parserService = parserService;
            ResponseUrl = responseUrl;
        }
        protected override IMessageResponse RespondTo(NotifyUserMessageArgument input)
        {
            switch(input.Payload.Type)
            {
                case NotifyUserMessageArgument.PRIVATEMESSAGETYPE:
                    return RespondToDirectMessage(input);
                case NotifyUserMessageArgument.PRIVATECHANNELMESSAGE:
                case NotifyUserMessageArgument.PUBLICCHANNELMESSAGETYPE:
                    return RespondToChannelMessage(input);
                default:
                    return new BasicResponse("Sorry, I do not know what you mean.");
            }
        }

        private IMessageResponse RespondToChannelMessage(NotifyUserMessageArgument input)
        {
            Task t = parserService.GetTask(input.Payload.Sender.Name, input.Payload.Message.Msg);
            if(t != null)
            {
                return new BasicResponse($"Created Task {t.ID}: {t.Title}", input.Payload.RoomId);
            } else
            {
                // todo: Write a more informing error message how the user can create a task
                return new BasicResponse("Did not understand", input.Payload.RoomId);
            }
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
            try
            {
                IEnumerable<Task> tl = GetTaskList(context, input.Title.Substring(1)).Where(x => !x.Done).ToList();
                if (tl.Count() < 1)
                    throw new InvalidOperationException("Tasklist is empty");
                
                return new BasicResponse(TaskListTemplate(tl.Select(x => new
                {
                    Task = x,
                    DoneLink = ResponseUrl + $"/Tasks/setDone/{x.ID}"
                })));
            }
            catch (InvalidOperationException)
            {
                return new BasicResponse("You do not have any open Tasks right now");
            }
            
        }

        private ICollection<Task> GetTaskList(TaskContext context, string username)
        {
            User user = context.Users.First(u => u.Name == username);
            IQueryable<UserTaskMap> utmaps = context.UserTaskMaps.Include(utm => utm.Task).Where(utm => utm.UserID == user.ID);
            List<Task> tl = utmaps.Select(utm => utm.Task).ToList();
            return tl;
        }
    }
}
