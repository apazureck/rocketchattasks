using Rocket.Chat.Net.Bot;
using Rocket.Chat.Net.Bot.Interfaces;
using Rocket.Chat.Net.Bot.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestBot
{
    public class TestResponse : IBotResponse
    {
        public bool CanRespond(ResponseContext context)
        {
            return false;
        }

        public IEnumerable<IMessageResponse> GetResponse(ResponseContext context, RocketChatBot caller)
        {
            return null;
        }
    }
}
