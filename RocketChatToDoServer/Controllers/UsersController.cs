﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.Database.Models;
using RocketChatToDoServer.TodoBot;

namespace RocketChatToDoServer.Controllers
{
    [Route("api/[controller]"), Authorize, ApiController]
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

        [HttpGet("filter/{search?}")]
        public IQueryable<User> GetFilteredUsers(string search = null)
        {
            IEnumerable<Rocket.Chat.Net.Models.FullUser> retusers;
            if (search == null)
                retusers = cache.Users;
             else
            {
                search = search.ToUpper();
                retusers = cache.Users.Where(u => u.Username.ToUpper().Contains(search));
            }
            
            return retusers.Select(u =>
                context.Users.FirstOrDefault(x => x.Name == u.Username) ?? new User
                    {
                        ID = 0,
                        Name = u.Username,
                        Tasks = new List<UserTaskMap>()
                    }).AsQueryable();
        }

        [HttpGet("currentlyLoggedIn")]
        public User GetUserByToken([FromHeader(Name = "Authorization")] string token)
        {
            return cache.GetUserByToken(token);
        }
    }
}