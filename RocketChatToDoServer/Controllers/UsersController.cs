using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.Database.Models;
using RocketChatToDoServer.TodoBot;

namespace RocketChatToDoServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly TaskContext context;
        private readonly BotService botService;

        public UsersController(TaskContext context, BotService botService)
        {
            this.context = context;
            this.botService = botService;
        }

        public IQueryable<User> Get() => context.Users.AsQueryable();

        [HttpGet("{id}")]
        public User Get(int id) => context.Users.First(x => x.ID == id);

        [HttpGet("filter/{search}")]
        public IQueryable<User> GetFilteredUsers(string search)
        {
            search = search.ToUpper();
            return context.Users.Where(u => u.Name.ToUpper().Contains(search));
        }
    }
}