using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RocketChatToDoServer.Database;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RocketChatToDoServer.Controllers
{
    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    [Route("api/auth"), ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly TodoBot.BotService botService;
        private readonly TaskContext context;
        private readonly RocketChatCache cache;

        public AuthController(IConfiguration configuration, TodoBot.BotService botService, TaskContext context, RocketChatCache cache)
        {
            this.configuration = configuration;
            this.botService = botService;
            this.context = context;
            this.cache = cache;
        }
        // GET api/values
        [HttpPost, Route("login")]
        public async Task<IActionResult> Login([FromBody]LoginModel user)
        {
            if (user == null)
            {
                return BadRequest("Invalid client request");
            }

            if (await botService.CheckUserLogin(user.UserName, user.Password))
            {
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue("jwtKey", Defaults.JwtKey)));
                var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

                var tokeOptions = new JwtSecurityToken(
                    issuer: "http://localhost:5000",
                    audience: "http://localhost:5000",
                    claims: new List<Claim>(),
                    expires: DateTime.Now.AddMinutes(5),
                    signingCredentials: signinCredentials
                );

                Database.Models.User fuser = context.Users.First(u => u.Name == user.UserName);

                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
                cache.AssignedTokens[tokenString] = fuser;
                return Ok(new { Token = tokenString, User = fuser });
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}