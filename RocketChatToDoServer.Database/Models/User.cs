using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RocketChatToDoServer.Database.Models
{
    public class User
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }

        public ICollection<Task> Tasks { get; set; }
    }
}
