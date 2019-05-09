using Microsoft.EntityFrameworkCore;
using RocketChatToDoServer.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RocketChatToDoServer.Database
{
    public class TaskRepository : IDisposable
    {
        private readonly TaskContext context;

        public TaskRepository(TaskContext context)
        {
            this.context = context;
        }
        IQueryable<User> Users { get; }

        #region Tasks

        public IQueryable<Task> Tasks => context.Tasks.AsQueryable();

        public void DeleteTask(int taskId)
        {
            Task task = context.Tasks.Where(t => t.ID == taskId)
                .Include(t => t.Assignees)
                .SingleOrDefault();
            if (task != null)
            {
                context.Tasks.Remove(task);
                context.SaveChanges();
            }
        }

        public void AddTask(Task task)
        {
            task.ID = 0;
            context.Tasks.Add(task);
            context.SaveChanges();
        }

        #endregion

        public void DeleteUser(int userId)
        {
            User user = context.Users.Where(u => u.ID == userId)
                .Include(u => u.Tasks)
                .SingleOrDefault();
            if (user != null)
            {
                context.Users.Remove(user);
                context.SaveChanges();
            }
        }

        public void AddTask(User user)
        {
            user.ID = 0;
            context.Users.Add(user);
            context.SaveChanges();
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
