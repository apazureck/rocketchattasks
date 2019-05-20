using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet("setDone/{taskId}")]
        public IActionResult SetDone(int taskId)
        {
            Task taskToSetToDone = context.Tasks.FirstOrDefault(t => t.ID == taskId);
            if(taskToSetToDone != null)
            {
                taskToSetToDone.Done = true;
                context.Update(taskToSetToDone);
                context.SaveChanges();
                return Ok($"Task {taskToSetToDone.Title} was set to done");
            } else
            {
                return NotFound();
            }
        }

        [HttpGet("forUser/{userId}")]
        public IQueryable<Task> GetOpenTasksForUser(int userId)
        {
            User user = context.Users.Include(x => x.Tasks)
                .ThenInclude(y => y.Task)
                .First(x => x.ID == userId);
            return user.Tasks.Select(x => x.Task).Where(x => !x.Done).AsQueryable();
        }
    }
}