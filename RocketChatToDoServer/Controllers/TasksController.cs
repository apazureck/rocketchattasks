using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.Database.Models;
using RocketChatToDoServer.TodoBot;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace RocketChatToDoServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskContext context;
        private readonly BotService botService;

        public TasksController(TaskContext context, BotService botService, IConfiguration config)
        {
            this.context = context;
            this.botService = botService;
        }
        public IQueryable<Task> Get()
        {
            return context.Tasks.AsQueryable();
        }

        [HttpGet("{id}")]
        public Task Get(int id)
        {
            return context.Tasks.Include(x=> x.Assignees).Include(x => x.Initiator).First(x => x.ID == id);
        }

        [HttpGet("forUser/{userId:int}/setDone/{taskId:int}")]
        public Task SetDone(int userId, int taskId)
        {
            return SetTaskTo(userId, taskId, true);
        }

        [HttpGet("forUser/{userId:int}/setUndone/{taskId:int}")]
        public Task SetUndone(int userId, int taskId)
        {
            return SetTaskTo(userId, taskId, false);
        }

        private Task SetTaskTo(int userId, int taskId, bool done)
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

            taskToSetToDone.Done = done;
            context.Update(taskToSetToDone);
            context.SaveChanges();

            SendDoneMessage(taskToSetToDone, done, assignee);
            return taskToSetToDone;
        }

        private async void SendDoneMessage(Task task, bool done, User assignee)
        {
            await botService.SendMessageToUser(assignee.ID, $"[Task {task.ID}]({Url+"/tasks/"+task.ID}): *{task.Title}* is " + (done ? "done" : "not done"));
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