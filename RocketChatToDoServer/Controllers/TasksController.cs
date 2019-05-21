using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.Database.Models;
using System;
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

        [HttpGet("forUser/{userId:int}/setDone/{taskId:int}")]
        public Task SetDone(int userId, int taskId)
        {
            User assignee = context.Users.FirstOrDefault(t => t.ID == userId);
            if (assignee == null)
                throw new ArgumentException($"User with ID {userId} was not found");

            Task taskToSetToDone = context.Tasks.Include(t => t.Assignees)
                .FirstOrDefault(t => t.ID == taskId);
            if (taskToSetToDone == null)
                throw new ArgumentException($"Task with ID {taskId} was not found");

            if (taskToSetToDone.InitiatorID != userId && !taskToSetToDone.Assignees.Any(tum => tum.UserID == userId))
                throw new InvalidOperationException("User is neither assigned nor the initiator of this task, cannot set to done.");

            taskToSetToDone.Done = true;
            context.Update(taskToSetToDone);
            context.SaveChanges();
            return taskToSetToDone;
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