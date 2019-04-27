using Microsoft.AspNetCore.Mvc;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.Database.Models;
using System.Linq;

namespace RocketChatToDoServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskContext context;

        public TasksController(TaskContext context)
        {
            this.context = context; 
        }
        public IQueryable<Task> Get()
        {
            return context.Tasks.AsQueryable();
        }
    }
}