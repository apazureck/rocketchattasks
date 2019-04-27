using Rocket.Chat.Net.Bot;
using Rocket.Chat.Net.Bot.Interfaces;
using Rocket.Chat.Net.Bot.Models;
using Rocket.Chat.Net.Interfaces;
using Rocket.Chat.Net.Models.LoginOptions;
using System;
using System.Threading.Tasks;

namespace TestBot
{
    class Program
    {
        private static RocketChatBot bot;
        static void Main(string[] args)
        {
            ILoginOption loginOption = new LdapLoginOption
            {
                Username = "todo.bot",
                Password = "EHEer7uwwYbsfGDFW"
            };

            // SetUp Bot
            bot = new RocketChatBot("192.168.99.100:5888", false, new Microsoft.Extensions.Logging.Console.ConsoleLogger("Logger", (a, b) => true, true));

            bot.AddResponse(new TestResponse());

            StartBot(loginOption);

            Console.ReadLine();
        }
        private static async void StartBot(ILoginOption option) {
            await bot.ConnectAsync();
            await bot.LoginAsync(option);
            await bot.SubscribeAsync(() => new DirectResponseMessage(), () => new DirectResponseMessage());
        }
    }

    public class DirectResponseMessage : Response<NotifyUserMessageArgument>
    {
        protected override IMessageResponse RespondTo(NotifyUserMessageArgument input)
        {
            Console.WriteLine("RESPONDING TO:");
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(input));
            return new BasicResponse(input.Text, null);
        }
    }
}
