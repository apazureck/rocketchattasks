using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RocketChatToDoServer
{
    public static class Defaults
    {
        public readonly static string JwtKey = Guid.NewGuid().ToString();
    }
}
