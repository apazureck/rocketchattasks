using System;
using System.Collections.Generic;
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
        [ForeignKey(nameof(Initiator))]
        public int InitiatorID { get; set; }
        public User Initiator { get; set; }
        public ICollection<UserTaskMap> Assignees { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Done { get; set; }
    }
}
