using Rocket.Chat.Net.Bot.Interfaces;
using Rocket.Chat.Net.Interfaces;
using Rocket.Chat.Net.Models;
using RocketChatToDoServer.Database.Models;
using RocketChatToDoServer.TodoBot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RocketChatToDoServer.Database
{
    public class RocketChatCache
    {
        public RocketChatCache()
        {
            
        }

        public void Setup(List<FullUser> users)
        {
            this.users = users;
        }

        private List<FullUser> users = new List<FullUser>();

        public IEnumerable<FullUser> Users => users;
        public IDictionary<string, Models.User> AssignedTokens { get; } = new Dictionary<string, Models.User>();
        public IDictionary<int, (string messageId, string roomId, IMessageResponse response)> LastTaskListMessageIds { get; } = new Dictionary<int, (string messageId, string roomId, IMessageResponse response)>();

        static readonly int toremove = "Bearer ".Length;
        public Models.User GetUserByToken(string token)
        {
            return AssignedTokens[token.Substring(toremove)];
        }
    }
}
