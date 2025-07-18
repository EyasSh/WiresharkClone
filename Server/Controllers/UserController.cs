using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Server.DB;
using Server.Models;
using Server.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using RestSharp;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/user")] // Base route for all actions in this controller
    public class UserController : ControllerBase
    {
        private readonly IHubContext<SocketService> _hubContext;
        IMongoCollection<User> _users;
        IMongoCollection<PacketInfo> _packets;
        IMongoCollection<VirusCheckerHistory> _virusCheckerHistory;
        IMongoCollection<Device> _devices;
        private readonly EmailService _emailService;
        private const string apiKey = "730392f1c1b50b2c0dd1ddac270b3802472f07bb3863282d02162322b8f76e22";
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
            _packets = dBWrapper.Packets;
            _virusCheckerHistory = dBWrapper.VirusCheckerHistory;
            _devices = dBWrapper.Devices;
        }

        /// <summary>
        /// Authenticates a user and provides access to the system. This action is accessible to anonymous users.
        /// </summary>
        /// <returns>A successful status code (200) if the login request was successful.</returns>
        [AllowAnonymous]
        [HttpPost("login")] // Route: api/user/login
        public async Task<IActionResult> Login([FromBody] Services.LoginRequest request)
        {
            try
            {
                await InitDevices();
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
                await InitDevices();
                var bytes = PdfGenerator.GenerateSimplePdfBytes();
                await _emailService.SendEmailWithAttachmentAsync(user.Email, "Welcome to The Service"
                ,
                $@"<html><body>Hello {user.Name}, <p>Welcome to Wire Tracer. 
                We're glad you're here. We're here to make packet analysis easier for you.</p>
                <br /> 
                <p>We hope you have a nice day!</p>
                The ReCoursia Team</body></html>", bytes, Guid.NewGuid().ToString() + ".pdf");

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
        /// <summary>
        /// Validates the password by comparing it with the hashed password stored in the database.
        /// </summary>
        /// <param name="plainText">The plaintext password</param>
        /// <param name="password">The hashed password</param>
        /// <returns>True if the password is valid, false otherwise</returns>
        private bool Validate(string plainText, string password) => BCrypt.Net.BCrypt.Verify(plainText, password);
        /// <summary>
        /// Validates the token sent in the X-Auth-Token header by checking that it was signed with the same key as the one used for signing the tokens.
        /// </summary>
        /// <returns>A successful status code (200) if the token is valid, or Unauthorized (401) if the token is invalid, or InternalServerError (500) if an error occurred.</returns>
        /// <remarks>
        /// This endpoint is used to verify that the token is valid, without logging in the user.
        /// </remarks>
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
        /// <summary>
        /// Uploads a file to VirusTotal and scans it for viruses.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <returns>A JSON response with the file name, number of engines that flagged the file as malicious, and a message indicating the result of the scan.</returns>
        /// <remarks>
        /// The file is uploaded to VirusTotal, and then the analysis results are polled until the scan is complete.
        /// The response includes the file name, the number of engines that flagged the file as malicious, and a message indicating the result of the scan.
        /// This endpoint is restricted to authorized users.
        /// </remarks>
        [Authorize]
        [HttpPost("file")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] string email, [FromForm] string userId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new JObject { ["message"] = "No file uploaded or file is empty." });
            }
            try
            {
                string uploadUrl = "https://www.virustotal.com/api/v3/files";
                string tempFilePath = Path.GetTempFileName();

                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var client = new RestClient(uploadUrl);
                var request = new RestRequest();
                request.Method = Method.Post;
                request.AddHeader("x-apikey", apiKey);
                request.AddFile("file", tempFilePath, file.ContentType);

                var uploadResponse = await client.ExecuteAsync(request);

                if (!uploadResponse.IsSuccessful)
                {
                    return StatusCode((int)uploadResponse.StatusCode,
                        new JObject { ["message"] = "Error uploading file to VirusTotal." });
                }

                if (uploadResponse.Content == null)
                {
                    return StatusCode(500, new JObject { ["message"] = "Error uploading file to VirusTotal." });
                }

                var uploadJson = JObject.Parse(uploadResponse.Content);
                string? analysisLink = uploadJson["data"]?["links"]?["self"]?.ToString();
                if (string.IsNullOrEmpty(analysisLink))
                {
                    return StatusCode(500, new JObject { ["message"] = "Error retrieving analysis link." });
                }

                var analysisClient = new RestClient(analysisLink);
                var analysisRequest = new RestRequest();
                analysisRequest.Method = Method.Get;
                analysisRequest.AddHeader("x-apikey", apiKey);

                var analysisResponse = await analysisClient.ExecuteAsync(analysisRequest);

                if (!analysisResponse.IsSuccessful)
                {
                    return StatusCode((int)analysisResponse.StatusCode,
                        new JObject { ["message"] = "Error retrieving analysis results." });
                }

                if (string.IsNullOrEmpty(analysisResponse.Content))
                {
                    return StatusCode(500, new JObject { ["message"] = "Error retrieving analysis results." });
                }

                var analysisJson = JObject.Parse(analysisResponse.Content);
                var stats = analysisJson["data"]?["attributes"]?["stats"];
                int malicious = stats?["malicious"]?.ToObject<int>() ?? 0;
                int undetected = stats?["undetected"]?.ToObject<int>() ?? 0;

                var resultMessage = new JObject
                {
                    ["fileName"] = file.FileName,
                    ["malicious"] = malicious,
                    ["undetected"] = undetected,
                    ["message"] = malicious > 0
                        ? "The file is flagged as malicious."
                        : "The file appears to be safe."
                };
                System.Console.WriteLine($"File: {file.FileName}, Malicious: {malicious}, Undetected: {undetected}");
                var pdf = PdfGenerator.GenerateFilePdf(file.FileName, malicious, undetected);
                await _emailService.SendEmailWithAttachmentAsync(
                    email,
                    $"Virus Scan Report for {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}",
                    $"<html><body><p>Please find the Virus Scan Report of {file.FileName} attached.</p></body></html>",
                    pdf,
                    $"{Guid.NewGuid()}_VirusScanReport.pdf"
                );
                System.Console.WriteLine("Email sent successfully.");
                if (malicious > 0)
                {
                    var historyRecord = new VirusCheckerHistory
                    {
                        Name = file.FileName,
                        Date = DateTime.Now,
                        Result = "Malicious",
                        UserId = userId
                    };
                    await _virusCheckerHistory.InsertOneAsync(historyRecord);
                }
                return Ok(resultMessage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new JObject { ["message"] = $"An error occurred: {ex.Message}" });
            }
        }
        /// <summary>
        /// Retrieves the history of virus scans.
        /// </summary>
        /// <returns>
        /// A successful status code (200) with the list of virus scan history records if the request was successful,
        /// or an internal server error status code (500) if an error occurred while retrieving the history.
        /// </returns>
        /// <remarks>
        /// The history contains the file name, date of scan, and the result of the scan (malicious or not).
        /// </remarks>
        [Authorize]
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] string? userId)
        {
            var history = await _virusCheckerHistory.Find(h => h.UserId == userId).ToListAsync();
            return Ok(history);
        }

        /// <summary>
        /// Validates the domain by retrieving its analysis results from VirusTotal.
        /// </summary>
        /// <param name="siteUri">The URI of the domain to validate.</param>
        /// <returns>
        /// A successful status code (200) with the analysis results if the request was successful,
        /// or a bad request status code (400) if the site URI is null or empty,
        /// or an internal server error status code (500) if an error occurred while validating the domain.
        /// </returns>
        [Authorize]
        [HttpPost("domain")]
        public async Task<IActionResult> DomainValidation([FromBody] string siteUri)
        {
            try
            {


                // Validate the URL
                if (string.IsNullOrWhiteSpace(siteUri))
                {

                    return BadRequest(new { message = "Site URI is required." });
                }

                // Step 1: Make the first API call to VirusTotal
                var options = new RestClientOptions("https://www.virustotal.com/api/v3/urls");
                var client = new RestClient(options);
                var request = new RestRequest("", Method.Post);
                request.AddHeader("accept", "application/json");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddHeader("x-apikey", apiKey);

                // Place the URL directly in the body as a form field
                string requestBody = $"url={Uri.EscapeDataString(siteUri)}"; // URL-encoded
                request.AddStringBody(requestBody, DataFormat.None);

                var response = await client.ExecuteAsync(request);

                // Step 2: Check the response from VirusTotal
                if (response == null)
                {

                    return StatusCode(500, new { message = "Failed to send the URL for analysis." });
                }

                if (response.StatusCode != HttpStatusCode.OK)
                {

                    return BadRequest(new { message = "Failed to send the URL for analysis.", details = response.Content });
                }

                Console.WriteLine($"VirusTotal Response: {response.Content}");
                if (string.IsNullOrEmpty(response.Content))
                {

                    return StatusCode(500, new { message = "Failed to send the URL for analysis. Response content is null" });
                }
                // Parse the response to retrieve the "self" link
                var responseJson = JObject.Parse(response.Content);
                string? selfLink = responseJson["data"]?["links"]?["self"]?.ToString();

                if (string.IsNullOrEmpty(selfLink))
                {

                    return BadRequest(new { message = "Failed to retrieve the analysis link." });
                }



                // Step 3: Make the second API call to the "self" link
                var analysisClient = new RestClient(new RestClientOptions(selfLink));
                var analysisRequest = new RestRequest("", Method.Get);
                analysisRequest.AddHeader("accept", "application/json");
                analysisRequest.AddHeader("x-apikey", apiKey);

                var analysisResponse = await analysisClient.ExecuteAsync(analysisRequest);

                // Check the analysis response
                if (analysisResponse == null || string.IsNullOrEmpty(analysisResponse.Content))
                {

                    return StatusCode(500, new { message = "Failed to retrieve the analysis results." });
                }

                if (analysisResponse.StatusCode != HttpStatusCode.OK)
                {

                    return BadRequest(new { message = "Failed to retrieve the analysis results.", details = analysisResponse.Content });
                }



                // Parse and return the analysis results
                var analysisJson = JObject.Parse(analysisResponse.Content);
                return Ok(new
                {
                    message = "Analysis completed successfully.",
                    results = analysisJson
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }
        [Authorize]
        [HttpPost("usage")]
        public async Task<IActionResult> Usage( PerformanceRequest request)
        {
            if (request.CpuUsage < 80 && request.RamUsage < 80 && request.DiskUsage < 80)
            {
                return Ok(new { message = "No excessive usage detected." });
            }
            var user = _users.FindSync(user => user.Email == request.Email).FirstOrDefault() ?? null;
            var pdf = PdfGenerator.GeneratePerformancePdf((request.CpuUsage, request.RamUsage, request.DiskUsage), user, request.AverageCpuUsage, request.AverageRamUsage, request.AverageDiskUsage);
            await _emailService.SendEmailWithAttachmentAsync(request.Email, $"{request.Name}, Usage Report for {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}",
           $@"
                <html>
                <body>
                Dear {request.Name},
                <p>This is a Performance Report for {DateOnly.FromDateTime(DateTime.Now)}.</p>
                <p>We usually send these requests when usage is above average. </p>
                <p>Thank you for using our service.</p>
                <p>Best regards,</p>
                <p>The Wire Tracer Team</p>
                </body>
                </html>
            ", pdf, user?.Id + "" + DateOnly.FromDateTime(DateTime.Now) + "" + ".pdf");
            return Ok();
        }
        [Authorize]
        [HttpGet("packets")]
        public Task<IActionResult> GetPackets()
        {
            var packets = _packets.Find(_ => true).ToList();
            return Task.FromResult<IActionResult>(Ok(packets));
        }
        /// <summary>
        /// Initializes the devices collection if it's empty.
        /// This method is mainly invoked during the Login and SignUp processes to ensure that the devices are saved with their usage averages in the database.
        /// </summary>
        private async Task InitDevices()
        {
            var devices = await _devices.Find(_ => true).ToListAsync();
            if (devices == null || devices.Count == 0)
            {
                devices = new List<Device>
                {
                    new Device { Name = "CPU", AverageUsage = 0.0, Counter = 0 },
                    new Device { Name = "RAM", AverageUsage = 0.0, Counter = 0 },
                    new Device { Name = "Disk", AverageUsage = 0.0, Counter = 0 }
                };
                await _devices.InsertManyAsync(devices);
            }
        }
    }
}
