using Microsoft.Extensions.Logging;
using Rocket.Chat.Net.Bot;
using Rocket.Chat.Net.Bot.Interfaces;
using Rocket.Chat.Net.Bot.Models;
using Rocket.Chat.Net.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestBot
{
    public static class RocketChatBotExtensions
    {
        private static readonly IReadOnlyList<string> streamNotifyAllMaps = new List<string>
            {
                "roles-change",
                "updateEmojiCustom",
                "deleteEmojiCustom",
                "updateAvatar",
                "public-settings-changed",
                "permissions-changed"
            }.AsReadOnly();

        public static async Task SubscribeToStreamNotifyAll(this RocketChatBot bot, StreamNotifyAllEvents @event, bool addEvent = false)
        {
            await bot.Driver.SubscribeToAsync("stream-notify-all", streamNotifyAllMaps[(int)@event], addEvent);
        }

        public static async Task SubscribeToStreamRoomMessages(this RocketChatBot bot, string room = null, bool addEvent = false)
        {
            await bot.Driver.SubscribeToRoomAsync(room);
        }
    }

    public enum StreamNotifyAllEvents
    {
        RolesChange,
        UpdateEmojiCustom,
        DeleteEmojiCustom,
        UpdateAvatar,
        PublicSettingsChanged,
        PermissionsChanged
    }

    public class CallbackBotResponse : IBotResponse
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
