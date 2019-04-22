using Microsoft.EntityFrameworkCore;
using RocketChatToDoServer.Database.Models;

namespace RocketChatToDoServer.Database
{
    public class TaskContext : DbContext
    {
        public TaskContext(DbContextOptions<TaskContext> options)
            : base(options)
        { }

        public DbSet<Task> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
