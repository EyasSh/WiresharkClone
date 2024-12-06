using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Server.DB;
using Server.Models;
namespace Server.Controllers
{
    [ApiController]
    [Route("api/user")] // Base route for all actions in this controller
    public class UserController : ControllerBase
    {
        IMongoCollection<User> _users;
        /*
         <summary>
         This is the user controller constructor to perform user actions
         using services such as an email service ETC
         </summary>
        */
        public UserController(MongoDBWrapper dBWrapper)
        {
            _users = dBWrapper.Users;
        }

        [AllowAnonymous]
        [HttpPost("/login")] // Route: api/user/login
        public IActionResult Login()
        {
            return Ok("Login successful.");
        }

        [AllowAnonymous]
        [HttpPost("signup")] // Route: api/user/signup
        public IActionResult SignUp()
        {
            return Ok("Sign-up successful.");
        }

        [HttpDelete("terminate/account")] // Route: api/user/terminate/account
        public IActionResult TerminateAccount()
        {
            return Ok("Account terminated.");
        }
    }
}
