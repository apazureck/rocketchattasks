using System.Threading.Tasks;

namespace RocketChatToDoServer.TodoBot
{
    public interface IPrivateMessenger
    {
        Task SendMessageToUser(int userId, string message);
    }
}