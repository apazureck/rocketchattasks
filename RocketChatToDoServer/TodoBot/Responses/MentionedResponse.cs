using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rocket.Chat.Net.Bot;
using Rocket.Chat.Net.Bot.Interfaces;
using Rocket.Chat.Net.Bot.Models;
using Rocket.Chat.Net.Models;
using Rocket.Chat.Net.Models.MethodResults;
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
        private readonly IPrivateMessenger messenger;
        private readonly RocketChatCache cache;

        public string ResponseUrl { get; }

        public MentionedResponse(ILogger<MentionedResponse> logger, TaskContext context, TaskParser.TaskParserService parserService, IPrivateMessenger messenger, RocketChatCache cache, string responseUrl = null)
        {
            this.logger = logger;
            this.context = context;
            this.parserService = parserService;
            this.messenger = messenger;
            this.cache = cache;
            ResponseUrl = responseUrl;
        }

        protected override IMessageResponse RespondTo(NotifyUserMessageArgument input)
        {
            switch (input.Payload.Type)
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
            Task t = parserService.GetTask(input.Payload.Sender.Username, input.Payload.Message.Msg);
            SendResponse(t);

            if (t != null)
                return new BasicResponse($"Created [Task {t.ID}]({CreateTaskUrl(t)}): {t.Title}", input.Payload.RoomId);
            else
            {
                // todo: Write a more informing error message how the user can create a task
                return new BasicResponse("Did not understand", input.Payload.RoomId);
            }
        }

        private void SendResponse(Task t)
        {
            if (t == null)
                return;
            // todo: Set to common messenger
            if (t.Assignees.Count == 1 && t.Assignees.First().UserID == t.InitiatorID)
                messenger.SendMessageToUser(t.InitiatorID, $"You assigned Task {t.ID} to yourself");
            else
                messenger.SendMessageToUser(t.InitiatorID, $"You assigned Task {t.ID} to " + t.Assignees.Select(x => x.UserID != t.InitiatorID ? "@" + x.User.Name : "yourself").Aggregate((a, b) => a + ", " + b));
            foreach (var assignee in t.Assignees)
            {
                if (assignee.UserID != t.InitiatorID)
                    messenger.SendMessageToUser(assignee.UserID, $"Task {t.ID} has been assigned to you by @" + t.Initiator.Name);
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
            if (response != null)
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

        private bool wasTaskListResponse = false;

        private IMessageResponse RespondToTaskList(NotifyUserMessageArgument input)
        {
            try
            {
                string username = input.Title.Substring(1);
                wasTaskListResponse = true;
                return new TaskListBuilder(context, ResponseUrl).GetMessage(username, $"Hello {username}, here you go:\n");
            }
            catch (InvalidOperationException)
            {
                return new BasicResponse("You do not have any open Tasks right now");
            }

        }

        public override void OnSuccess(MethodResult<RocketMessage> result, IMessageResponse response)
        {
            if (wasTaskListResponse && response is TasklistMessageResponse tlmr)
            {
                cache.LastTaskListMessageIds[tlmr.GetUser().ID] = (result.Result.Id, result.Result.RoomId, response);
            }
        }

        private string CreateTaskUrl(Task t) => $"{ResponseUrl}/tasks/{t.ID}";
    }
}
