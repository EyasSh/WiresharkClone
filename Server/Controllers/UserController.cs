using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Encryption;
using Server.DB;
using Server.Models;
using Server.Services;
using BCrypt;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.SignalR;
namespace Server.Controllers
{
    [ApiController]
    [Route("api/user")] // Base route for all actions in this controller
    public class UserController : ControllerBase
    {
        private readonly IHubContext<SocketService> _hubContext;
        IMongoCollection<User> _users;
        private readonly EmailService _emailService;
        private readonly IConfiguration _conf;
        /*
         <summary>
         This is the user controller constructor to perform user actions
         using services such as an email service ETC
         </summary>
        */
        public UserController(MongoDBWrapper dBWrapper, IConfiguration conf
        , IHubContext<SocketService> hubContext, EmailService emailService
        )
        {
            _users = dBWrapper.Users;
            _conf = conf;
            _hubContext = hubContext;
            _emailService = emailService;
        }

        /// <summary>
        /// Authenticates a user and provides access to the system. This action is accessible to anonymous users.
        /// </summary>
        /// <returns>A successful status code (200) if the login request was successful.</returns>
        [AllowAnonymous]
        [HttpPost("login")] // Route: api/user/login
        public IActionResult Login([FromBody] Services.LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest("Invalid login credentials.");
                }
                var user = _users.Find(user => user.Email == request.Email).FirstOrDefault();


                if (user == null)
                {
                    return BadRequest("User not found.");
                }
                if (user != null && Validate(request.Password, user.Password))
                {
                    var token = GenerateJwtToken(user.Id ?? new Guid().ToString(), request.Email);
                    var resbod = new User
                    { Id = user.Id, Name = user.Name, Email = user.Email, Password = user.Password, date = user.date };
                    Response.Headers["X-Auth-Token"] = token;
                    return Ok(new { User = resbod });
                }
                else
                {

                    return BadRequest("Invalid login credentials or user not found." + $"{user?.Email ?? "No email or user found"}\nplain pass encrypted:{Encrypt(request.Password)}\nalready encrypted:{user?.Password ?? "No password found"}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

        }
        /// <summary>
        /// Generates a JWT token with the given username.
        /// </summary>
        /// <param name="email">The email to include in the JWT token.</param>
        /// <returns>The JWT token.</returns>
        /// <remarks>
        /// The JWT token is generated with the following settings:
        /// <list type="bullet">
        /// <item>
        /// <term>Issuer</term>
        /// <description>The value of the <c>Jwt:Issuer</c> configuration setting.</description>
        /// </item>
        /// <item>
        /// <term>Audience</term>
        /// <description>The value of the <c>Jwt:Audience</c> configuration setting.</description>
        /// </item>
        /// <item>
        /// <term>Claims</term>
        /// <description>
        /// A list of claims containing the username and a unique identifier.
        /// </description>
        /// </item>
        /// <item>
        /// <term>Expiration</term>
        /// <description>The token will expire in 30 Days.</description>
        /// </item>
        /// <item>
        /// <term>Signature</term>
        /// <description>
        /// The token is signed with the key specified in the <c>Jwt:Key</c> configuration setting.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        private string GenerateJwtToken(string Id, string email)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_conf["Jwt:Key"] ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti,Id )
            };

            var token = new JwtSecurityToken(
                issuer: _conf["Jwt:Issuer"],
                audience: _conf["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        /// <summary>
        /// Sign-up a new user using the provided user credentials. This action
        /// is accessible to anonymous users.
        /// </summary>
        /// <returns>A successful status code (200) if the sign-up request was
        /// successful.</returns>

        [AllowAnonymous]
        [HttpPost("signup")] // Route: api/user/signup
        public async Task<IActionResult> SignUp([FromBody] SignupRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.date.ToString()))
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
                    Password = Encrypt(request.Password),
                    date = request.date
                };
                _users.InsertOne(user);
                await _emailService.SendEmailAsync(user.Email, "Welcome to The Service"
                ,
                $@"<html><body>Hello {user.Name}, <p>Welcome to Wire Tracer. 
                We're glad you're here. We're here to make packet analysis easier for you.</p>
                <br /> 
                <p>We hope you have a nice day!</p>
                The ReCoursia Team</body></html>");

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
        [Authorize]
        [HttpDelete("terminate/account/{email}")] // Route: api/user/terminate/account
        public IActionResult TerminateAccount(string email)
        {
            var user = _users.FindSync(user => user.Email == email).FirstOrDefault();
            if (user == null || (user is not null && string.IsNullOrEmpty(user.Email)))
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
        [AllowAnonymous]
        [HttpGet("validate")]
        public IActionResult ValidateToken()
        {
            if (!Request.Headers.ContainsKey("X-Auth-Token"))
            {
                System.Console.WriteLine("in first if");
                return BadRequest("Token is missing.");
            }

            var token = Request.Headers["X-Auth-Token"].ToString();

            try
            {

                // Access the authentication scheme's options
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _conf["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_conf["Jwt:Key"] ?? string.Empty))
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

                // Token is valid, return success with claims if needed
                var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;
                var idClaim = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                return Ok(new { Message = "Token is valid", Email = emailClaim, Id = idClaim });
            }
            catch (SecurityTokenException ex)
            {

                return Unauthorized($"Token validation failed: {ex.Message}");
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

    }
}
