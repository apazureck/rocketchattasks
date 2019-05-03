using Microsoft.Extensions.Logging;
using Rocket.Chat.Net.Bot;
using Rocket.Chat.Net.Interfaces;
using System;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Chat.Net.Driver;
using System.Threading.Tasks;
using System.Collections.Generic;
using Rocket.Chat.Net.Bot.Interfaces;

namespace RocketChatToDoServer.TodoBot
{
    public class RcDiBot : RocketChatBot
    {
        private IServiceProvider provider;
        private readonly IServiceCollection services;

        public RcDiBot(string url, bool useSsl, IServiceCollection services, ILogger logger = null)
            : this(new RocketChatDriver(url, useSsl, logger), logger, services)
        {
        }

        public RcDiBot(IRocketChatDriver driver, ILogger logger, IServiceCollection services) : base(driver, logger)
        {
            this.services = services;
        }

        public async Task SubscribeAsync<T>(Stream stream, Func<IServiceProvider, RcDiBot, IResponse> factory = null, params object[] parameters) where T : class, IResponse
        {
            string subscriptionId = await this.SubscribeAsync(stream, parameters);
            responses.Add(subscriptionId, typeof(T));
            if (factory == null)
                services.AddScoped<T>();
            else
                services.AddScoped(p => (T)factory(p, this));
            provider = services.BuildServiceProvider();
        }

        private Dictionary<string, Type> responses = new Dictionary<string, Type>();

        override protected async Task ProcessRequest(string subscriptionId, List<Newtonsoft.Json.Linq.JObject> fields)
        {
            using (IServiceScope scope = provider.CreateScope())
            {
                try
                {
                    var response = (IResponse)scope.ServiceProvider.GetService(responses[subscriptionId]);
                    IMessageResponse message = response.RespondTo(fields);
                    await SendMessageAsync(message);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred when processing request {subid} with payload {args}", subscriptionId, fields);
                }
            }
        }
    }
}
