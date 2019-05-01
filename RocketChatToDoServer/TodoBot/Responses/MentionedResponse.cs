using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rocket.Chat.Net.Bot;
using Rocket.Chat.Net.Bot.Interfaces;
using Rocket.Chat.Net.Bot.Models;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RocketChatToDoServer.TodoBot.Responses
{
    public class MentionedResponse : Response<NotifyUserMessageArgument>
    {
        private readonly ILogger logger;
        private readonly TaskContext context;

        public MentionedResponse(ILogger<MentionedResponse> logger, TaskContext context)
        {
            this.logger = logger;
            this.context = context;
        }
        protected override IMessageResponse RespondTo(NotifyUserMessageArgument input)
        {
            if(input.IsDirectMessage)
            {
                if(input.Payload.Message.Msg.ToUpper().Contains("TASKLIST"))
                {
                    return new BasicResponse("**Your Tasks:**\n" + GetTaskList(context, input.Title.Substring(1)).Select(t => t.ID + " " + t.TaskDescription + (t.DueDate != default ? "; DUE: " + t.DueDate : "")).Aggregate("", (a, b) => a + $"\n- " + b), input.Payload.Rid);
                }
                return new BasicResponse("Hey, I received private message:\n```" + Newtonsoft.Json.JsonConvert.SerializeObject(input) + "\n```");
            }


            return new BasicResponse("Hey, I was mentioned:\n```" + Newtonsoft.Json.JsonConvert.SerializeObject(input) + "\n```");
        }

        private ICollection<Database.Models.Task> GetTaskList(TaskContext context, string username) => context.Users.Include(u => u.Tasks).First(x => x.Name == username).Tasks;
    }
}
