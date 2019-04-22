using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RocketChatToDoServer.Database.Models
{
    public class Task
    {
        [Key]
        public int ID { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreationDate { get; set; }
        [ForeignKey(nameof(User))]
        public int UserID { get; set; }
        public User User { get; set; }
        public string TaskDescription { get; set; }
        public bool Done { get; set; }
    }
}
