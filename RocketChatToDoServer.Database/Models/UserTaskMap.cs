using System.ComponentModel.DataAnnotations;

namespace RocketChatToDoServer.Database.Models
{
    public class UserTaskMap
    {
        [Key]
        public int UserID { get; set; }
        public User User { get; set; }
        [Key]
        public int TaskID { get; set; }
        public Task Task { get; set; }
    }
}