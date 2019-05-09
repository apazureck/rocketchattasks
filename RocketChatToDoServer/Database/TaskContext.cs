using Microsoft.EntityFrameworkCore;
using RocketChatToDoServer.Database.Models;

namespace RocketChatToDoServer.Database
{
    public class TaskContext : DbContext
    {
        public TaskContext(DbContextOptions<TaskContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserTaskMap>().HasKey(x => new
            {
                x.TaskID,
                x.UserID
            });
        }

        public DbSet<Task> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserTaskMap> UserTaskMaps { get; set; }
    }
}
