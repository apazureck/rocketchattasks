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
        private readonly RocketChatCache cache;

        public UsersController(TaskContext context, BotService botService, RocketChatCache cache)
        {
            this.context = context;
            this.botService = botService;
            this.cache = cache;
        }

        public IQueryable<User> Get() => context.Users.AsQueryable();

        [HttpGet("{id}")]
        public User Get(int id) => context.Users.First(x => x.ID == id);

        [HttpGet("filter/{search}")]
        public async Task<IQueryable<User>> GetFilteredUsers(string search)
        {
            search = search.ToUpper();
            await cache.Setup();
            return cache.Users.Where(u => u.Username.ToUpper().Contains(search)).Select(u =>
                context.Users.FirstOrDefault(x => x.Name == u.Username) ?? new User
                    {
                        ID = 0,
                        Name = u.Username,
                        Tasks = new List<UserTaskMap>()
                    }).AsQueryable();
        }
    }
}