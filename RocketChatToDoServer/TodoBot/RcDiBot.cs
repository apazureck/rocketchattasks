using Microsoft.Extensions.Logging;
using Rocket.Chat.Net.Bot;
using Rocket.Chat.Net.Interfaces;
using System;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Chat.Net.Driver;
using System.Threading.Tasks;
using System.Collections.Generic;
using Rocket.Chat.Net.Bot.Interfaces;
using RocketChatToDoServer.Database;
using System.Linq;

namespace RocketChatToDoServer.TodoBot
{
    public class RcDiBot : RocketChatBot
    {
        private IServiceProvider provider;
        private readonly IServiceCollection services;

        public RcDiBot(string url, bool useSsl, IServiceCollection services, ILogger logger = null, string responseAddress = null)
            : this(new RocketChatDriver(url, useSsl, logger), logger, services, responseAddress)
        {
        }

        public RcDiBot(IRocketChatDriver driver, ILogger logger, IServiceCollection services, string responseAddress = null) : base(driver, logger)
        {
            ResponseUrl = responseAddress;
            this.services = services;
        }

        private class Subscription
        {
            public Subscription(string subscriptionId, Stream stream, object[] parameters, Type responseType)
            {
                SubscriptionId = subscriptionId;
                Stream = stream;
                Parameters = parameters;
                ResponseType = responseType;
            }
            public string SubscriptionId { get; set; }
            public Stream Stream { get; }
            public object[] Parameters { get; }
            public Type ResponseType { get; }
        }

        private Dictionary<string, Subscription> subscriptions = new Dictionary<string, Subscription>();

        public async Task SubscribeAsync<T>(Stream stream, Func<IServiceProvider, RcDiBot, IResponse> factory = null, params object[] parameters) where T : class, IResponse
        {
            string subscriptionId = await this.SubscribeAsync(stream, parameters);
            subscriptions.Add(subscriptionId, new Subscription(subscriptionId, stream, parameters, typeof(T)));
            if (factory == null)
                services.AddScoped<T>();
            else
                services.AddScoped(p => (T)factory(p, this));
            provider = services.BuildServiceProvider();
        }

        public string ResponseUrl { get; }

        internal async Task SendMessageAsync(string message, int userId)
        {
            using(IServiceScope scope = provider.CreateScope())
            {
                TaskContext context = scope.ServiceProvider.GetService<TaskContext>();
                Database.Models.User user = context.Users.First(u => u.ID == userId);
                var pm = await Driver.CreatePrivateMessageAsync(user.Name);
                var result = await Driver.SendMessageAsync(message, pm.Result.RoomId);
            }
        }

        override protected async Task ProcessRequest(string subscriptionId, List<Newtonsoft.Json.Linq.JObject> fields)
        {
            using (IServiceScope scope = provider.CreateScope())
            {
                try
                {
                    var response = (IResponse)scope.ServiceProvider.GetService(subscriptions[subscriptionId].ResponseType);
                    IMessageResponse message = response.RespondTo(fields);
                    await SendMessageAsync(message);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred when processing request {subid} with payload {args}", subscriptionId, fields);
                }
            }
        }

        protected override async Task OnResumedAsync()
        {
            var subs = subscriptions.Values.ToList();

            subscriptions.Clear();
            foreach(var sub in subs)
            {
                sub.SubscriptionId = await this.SubscribeAsync(sub.Stream, sub.Parameters);
                subscriptions.Add(sub.SubscriptionId, sub);
            }
        }
    }
}
