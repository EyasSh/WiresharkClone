using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Encryption;
using PcapDotNet.Base;
using Server.DB;
using Server.Models;
using Server.Services;
using BCrypt;
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

        /// <summary>
        /// Authenticates a user and provides access to the system. This action is accessible to anonymous users.
        /// </summary>
        /// <returns>A successful status code (200) if the login request was successful.</returns>
        [AllowAnonymous]
        [HttpPost("/login")] // Route: api/user/login
        public IActionResult Login([FromBody] Services.LoginRequest request)
        {
            try
            {
                if (request.Email.IsNullOrEmpty() || request.Password.IsNullOrEmpty())
                {
                    return BadRequest("Invalid login credentials.");
                }
                var user = _users.Find(user => user.Email == request.Email).FirstOrDefault();
                if (user == null)
                {
                    return BadRequest("User not found.");
                }
                if (user is not null && Validate(request.Password, user.Password))
                {
                    return Ok("Login successful.");
                }
                else
                {

                    return BadRequest("Invalid login credentials or user not found." + $"{user.Email}\nplain pass encrypted:{Encrypt(request.Password)}\nalrady encrypted:{user.Password}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

        }


        /// <summary>
        /// Sign-up a new user using the provided user credentials. This action
        /// is accessible to anonymous users.
        /// </summary>
        /// <returns>A successful status code (200) if the sign-up request was
        /// successful.</returns>

        [AllowAnonymous]
        [HttpPost("/signup")] // Route: api/user/signup
        public IActionResult SignUp([FromBody] SignupRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest("All fields are required.");
                }

                if (_users.FindSync(user => user.Email == request.Email).Any())
                {
                    return BadRequest("User already exists.");
                }

                var user = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    Password = Encrypt(request.Password)
                };
                _users.InsertOne(user);

                return Ok("Sign-up successful.");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
                return StatusCode(500, $"An error occurred: {ex.Message} at signup");
            }
        }



        /// <summary>
        /// Terminates the user account. This action is restricted to authorized users.
        /// </summary>
        /// <returns>A successful status code (200) if the account termination was successful.</returns>

        [HttpDelete("/terminate/account/{email}")] // Route: api/user/terminate/account
        public IActionResult TerminateAccount(string email)
        {
            var user = _users.FindSync(user => user.Email == email).FirstOrDefault();
            if (user == null || (user is not null && user.Email.IsNullOrEmpty()))
            {
                return BadRequest("User not found.");
            }
            _users.DeleteOne(user => user.Email == email);
            return Ok("Account terminated.");
        }
        /// <summary>
        /// A test endpoint to verify that the API is properly deployed.
        /// </summary>
        /// <returns>A successful status code (200) if the test request was successful.</returns>
        [HttpGet("test")]
        public IActionResult Test() => Ok("Test Successful");
        private string Encrypt(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        private bool Validate(string plainText, string password) => BCrypt.Net.BCrypt.Verify(plainText, password);
    }
}
