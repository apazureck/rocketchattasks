using Rocket.Chat.Net.Interfaces;
using Rocket.Chat.Net.Models;
using RocketChatToDoServer.TodoBot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RocketChatToDoServer.Database
{
    public class RocketChatCache
    {
        public RocketChatCache(BotService botService)
        {
            this.botService = botService;
        }

        public async Task Setup()
        {
            users = await botService.GetUserList();
        }

        private List<FullUser> users = new List<FullUser>();
        private readonly BotService botService;

        public IEnumerable<FullUser> Users => users;
    }
}
