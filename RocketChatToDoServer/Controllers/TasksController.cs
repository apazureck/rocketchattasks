using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.Database.Models;
using RocketChatToDoServer.TodoBot;
using System;
using System.Linq;
using Async = System.Threading.Tasks;

namespace RocketChatToDoServer.Controllers
{
    [Route("api/[controller]"), Authorize, ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskContext context;
        private readonly BotService botService;
        private readonly ILogger<TasksController> logger;
        private readonly RocketChatCache cache;
        private readonly string responseUrl;

        public TasksController(TaskContext context, BotService botService, IConfiguration config, ILogger<TasksController> logger, RocketChatCache cache)
        {
            this.context = context;
            this.botService = botService;
            this.logger = logger;
            this.cache = cache;
            responseUrl = config.GetSection("bots").Get<BotConfiguration[]>()[0].ResponseUrl;
        }
        
        [HttpGet]
        public IQueryable<Task> Get()
        {
            logger.LogTrace("Getting all tasks");
            return context.Tasks.AsQueryable();
        }

        [HttpGet("{id}")]
        public Task Get(int id)
        {
            logger.LogTrace("Getting Task with ID {id}", id);
            var task = context.Tasks.Include(x => x.Assignees).ThenInclude(m => m.User).Include(x => x.Initiator).First(x => x.ID == id);
            return task;
        }

        [HttpPost("{taskId:int}/setDone")]
        public Task PostDone(int taskId, [FromBody] int userId)
        {
            logger.LogTrace("Setting Task {id} to done", taskId);
            return SetTaskTo(userId, taskId, true);
        }

        [HttpPost("{taskId:int}/setUndone")]
        public Task PostUndone(int taskId, [FromBody] int userId)
        {
            logger.LogTrace("Setting Task {id} to open", taskId);
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
            UpdateLastTaskList(taskToSetToDone, done, assignee);
            return taskToSetToDone;
        }

        private async void UpdateLastTaskList(Task taskToSetToDone, bool done, User assignee)
        {
            logger.LogTrace("Updating tasklist for user {assignee}", assignee.ID);
            await botService.UpdateTaskList(assignee.ID, taskToSetToDone);
        }

        private async void SendDoneMessage(Task task, bool done, User assignee)
        {
            logger.LogTrace("Sending done message to user {assignee}", assignee.ID);
            await botService.SendMessageToUser(assignee.ID, $"[Task {task.ID}]({responseUrl+"/tasks/"+task.ID}): *{task.Title}* is " + (done ? "done" : "not done"));
        }

        [HttpGet("forUser/{userId}")]
        public IQueryable<Task> GetOpenTasksForUser(int userId)
        {
            logger.LogTrace("Getting open tasks for user {id}", userId);
            User user = context.Users.Include(x => x.Tasks)
                .ThenInclude(y => y.Task)
                .First(x => x.ID == userId);
            return user.Tasks.Select(x => x.Task).Where(x => !x.Done).AsQueryable();
        }

        [HttpPost("{id}/addAssignee")]
        public async Async.Task<IActionResult> AddUserToTask(int id, [FromBody] User user)
        {
            logger.LogTrace("Adding user {uid} to task {tid}", user.ID, id);
            Task task = context.Tasks.FirstOrDefault(t => t.ID == id);
            User initiator = null;

            if (task == null)
                return NotFound("Task was not found");

            if(user.ID == 0)
            {
                user = context.Users.Add(user).Entity;
                context.SaveChanges();
            }

            try
            {
                context.UserTaskMaps.Add(new UserTaskMap
                {
                    UserID = user.ID,
                    TaskID = id
                });
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                return BadRequest("Assignee is already assigned");
            }

            await botService.SendAssigneeMessage(user, task, initiator);

            return Ok(context.Tasks.Include(t => t.Assignees).First(t => t.ID == task.ID));
        }

        [HttpPost("{id}/removeAssignee")]
        public async Async.Task<IActionResult> RemoveUserFromTask(int id, [FromBody] int userId)
        {
            logger.LogTrace("Removing user {id} from task {task}", userId, id);
            UserTaskMap assignee = context.UserTaskMaps.Find(id, userId);
            User initiator = null;
            if (assignee == null)
                return NotFound("Assignee was not found");

            context.UserTaskMaps.Remove(assignee);
            context.SaveChanges();

            await botService.SendUnAssignedMessage(context.Users.FirstOrDefault(x => x.ID == userId), context.Tasks.FirstOrDefault(x => x.ID == id), initiator);

            return Ok();
        }

        [HttpPut]
        public async Async.Task<IActionResult> UpdateTask([FromBody] Task task, [FromHeader(Name = "Authorization")] string token)
        {
            logger.LogTrace("updating task {task}", task.ID);

            // Simple check if entity is present
            if (task.ID == 0)
                return BadRequest();

            User loggedInUser = cache.GetUserByToken(token);

            Task original = context.Tasks.Include(t => t.Initiator).Include(t => t.Assignees).FirstOrDefault(x => x.ID == task.ID);

            // Check if user is allowed to edit task
            if (UserIsNotAllowedToEditTask(loggedInUser, original))
                return BadRequest("Only assignees or the initiator are allowed to change the task");

            if (original.Done != task.Done)
                SetTaskTo(loggedInUser.ID, task.ID, task.Done);

            context.Tasks.Update(task);

            context.SaveChanges();

            task = context.Tasks.Include(x => x.Assignees).ThenInclude(x => x.User).Include(x => x.Initiator).FirstOrDefault(x => x.ID == task.ID);

            // Send update message, but ignore errors
            try
            {
                await botService.SendTaskUpdatedMessage(task, loggedInUser);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending Message to currently logged in user with token {token}", token);
            }

            return Ok(task);
        }

        private static bool UserIsNotAllowedToEditTask(User loggedInUser, Task task)
        {
            return task.Initiator.ID != loggedInUser.ID && !task.Assignees.Any(u => u.UserID == loggedInUser.ID);
        }
    }
}