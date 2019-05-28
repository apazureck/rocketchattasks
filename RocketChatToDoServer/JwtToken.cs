using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RocketChatToDoServer
{
    public static class Defaults
    {
        public static string JwtKey { get; } = Guid.NewGuid().ToString();
    }
}
