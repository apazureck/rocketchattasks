using Microsoft.Extensions.Logging;
using Rocket.Chat.Net.Bot;
using Rocket.Chat.Net.Bot.Interfaces;
using Rocket.Chat.Net.Bot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RocketChatToDoServer.TodoBot.Responses
{
    public class TaskListResponse : IBotResponse
    {
        private readonly ILogger logger;

        public TaskListResponse(ILogger logger)
        {
            this.logger = logger;
        }
        public bool CanRespond(ResponseContext context)
        {
            logger.LogInformation(context.Message.Message);
            return false;
        }

        public IEnumerable<IMessageResponse> GetResponse(ResponseContext context, RocketChatBot caller)
        {
            return new List<IMessageResponse>();
        }
    }
}
