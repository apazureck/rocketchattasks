using Microsoft.Extensions.Logging;
using Rocket.Chat.Net.Bot;
using Rocket.Chat.Net.Bot.Interfaces;
using Rocket.Chat.Net.Bot.Models;

namespace RocketChatToDoServer.TodoBot.Responses
{
    public class DirectResponseMessage : Response<NotifyUserMessageArgument>
    {
        private readonly ILogger logger;

        public DirectResponseMessage(ILogger logger)
        {
            this.logger = logger;
        }
        protected override IMessageResponse RespondTo(NotifyUserMessageArgument input)
        {
            return new BasicResponse("Hey, I am ready!");
        }
    }
}
