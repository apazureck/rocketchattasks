using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RocketChatToDoServer.Database;

namespace RocketChatToDoServer.Controllers
{
    [Route("chat/[controller]")]
    [ApiController]
    public class RocketChatInputController : ControllerBase
    {
        private readonly ILogger<RocketChatInputController> logger;
        private readonly TaskParser taskParser;

        public RocketChatInputController(ILogger<RocketChatInputController> logger, TaskParser taskParser)
        {
            this.logger = logger;
            this.taskParser = taskParser;
        }
        public ActionResult Post(RocketChatRequest data)
        {
            taskParser.ParseMessage(data.data.text, logger, DateTime.Now);
            return Ok();
        }
    }

    public class RocketChatRequest
    {
        // request.params            {object}
        // request.method            {string}
        public string method { get; set; }
        // request.url               {string}
        public string url { get; set; }
        // request.auth              {string}
        public string auth { get; set; }
        // request.headers           {object}
        public RocketChatRequestData data { get; set; }
    }

    public class RocketChatRequestData
    {
        // request.data.token        {string}
        public string token { get; set; }
        // request.data.channel_id   {string}
        public string channel_id { get; set; }
        // request.data.channel_name {string}
        public string channel_name { get; set; }
        // request.data.timestamp    {date}
        public string timestamp { get; set; }
        // request.data.user_id      {string}
        public string user_id { get; set; }
        // request.data.user_name    {string}
        public string user_name { get; set; }
        // request.data.text         {string}
        public string text { get; set; }
        // request.data.trigger_word {string}
        public string trigger_word { get; set; }
    }
}