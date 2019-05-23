using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.Database.Models;

namespace RocketChatToDoServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly TaskContext context;

        public UsersController(TaskContext context)
        {
            this.context = context;
        }

        public IQueryable<User> Get() => context.Users.AsQueryable();

        [HttpGet("{id}")]
        public User Get(int id) => context.Users.First(x => x.ID == id);
    }
}