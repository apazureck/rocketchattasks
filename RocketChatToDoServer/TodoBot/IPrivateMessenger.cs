using System.Threading.Tasks;

namespace RocketChatToDoServer.TodoBot
{
    public interface IPrivateMessenger
    {
        Task SendMessageToUser(int userId, string message);
        Task SendAssigneeMessage(Database.Models.User user, Database.Models.Task task, Database.Models.User initiator);
    }
}