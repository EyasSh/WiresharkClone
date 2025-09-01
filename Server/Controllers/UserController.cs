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
using Org.BouncyCastle.Bcpg;

namespace Server.Controllers
{
    /// <summary>
    /// Controller for managing user-related actions.
    /// </summary>
    [ApiController]
    [Route("api/user")]
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
                await SendWelcomeEmail(user.Email, user.Name);
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
        public async Task<IActionResult> DomainValidation([FromBody] DomainValidationRequest req)
        {
            try
            {

                if (req == null)
                {
                    return BadRequest(new { message = "Request cannot be null." });
                }
                if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Url))
                {
                    return BadRequest(new { message = "Email and URL are required." });
                }
                // Validate the URL
                if (string.IsNullOrWhiteSpace(req.Url))
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
                string requestBody = $"url={Uri.EscapeDataString(req.Url)}"; // URL-encoded
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
                // analysisJson is a JObject produced by JObject.Parse(jsonString)
                var statsToken = analysisJson["data"]?["attributes"]?["stats"];
                int malicious = statsToken?["malicious"]?.Value<int>() ?? 0;
                int suspicious = statsToken?["suspicious"]?.Value<int>() ?? 0;
                int undetected = statsToken?["undetected"]?.Value<int>() ?? 0;
                int harmless = statsToken?["harmless"]?.Value<int>() ?? 0;
                int timeout = statsToken?["timeout"]?.Value<int>() ?? 0;

                await SendLinkEmail(
                    req.Email,
                    req.Url,
                    timeout,
                    undetected,
                    malicious,
                    harmless,
                    suspicious
                );
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
        /// <summary>
        /// Handles the performance usage report.
        /// </summary>
        /// <param name="request">A <see cref="PerformanceRequest"/> object containing the CPU, RAM, and disk usage, the user's name and email, as well as the average CPU, RAM, and disk usage.</param>
        /// <returns>A JSON response with a message indicating whether the usage is excessive or not. If excessive, an email with a PDF attachment is sent to the user.</returns>
        [Authorize]
        [HttpPost("usage")]
        public async Task<IActionResult> Usage(PerformanceRequest request)
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

        /// <summary>
        /// Retrieves the packets for the given user ID.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve packets for.</param>
        /// <returns>A JSON response containing the packets.</returns>
        [Authorize]
        [HttpGet("packets")]
        public async Task<IActionResult> GetPackets([FromQuery] PacketPageRequest q)
        {
            var filter = Builders<PacketInfo>.Filter.Eq(p => p.UserId, q.UserId);

            // optional total
            long? total = null;
            if (q.IncludeTotal)
                total = await _packets.CountDocumentsAsync(filter);

            var items = await _packets.Find(filter)
                                      .SortByDescending(p => p.Timestamp) // if you have such a field
                                      .Skip(q.Skip)
                                      .Limit(q.Limit)
                                      .ToListAsync();

            return Ok(new
            {
                items,
                total,               // null if not requested
                pageNumber = q.PageNumber,
                pageSize = q.PageSize
            });
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
        /// <summary>
        /// Sends an email containing a domain scan report with the analysis results.
        /// </summary>
        /// <param name="email">The recipient's email address.</param>
        /// <param name="url">The URL that was scanned.</param>
        /// <param name="timeout">The number of engines that timed out during the scan.</param>
        /// <param name="undetected">The number of engines that did not detect any issues.</param>
        /// <param name="malicious">The number of engines that flagged the URL as malicious.</param>
        /// <param name="harmless">The number of engines that marked the URL as harmless.</param>
        /// <param name="suspicious">The number of engines that flagged the URL as suspicious.</param>
        /// <remarks>
        /// Constructs an HTML formatted email with a summary of the scan results and sends it using the email service.
        /// </remarks>
        private async Task SendLinkEmail(
     string email,
     string url,
     int timeout,
     int undetected,
     int malicious,
     int harmless,
     int suspicious)
        {
            var summary = malicious > 0
                ? $"⚠️ Heads up: {malicious} engine{(malicious == 1 ? "" : "s")} flagged this URL."
                : "✅ Good news: No engines flagged this URL as malicious.";

            string HtmlEscape(string s) => System.Net.WebUtility.HtmlEncode(s);

            var urlDisplay = HtmlEscape(url);

            string htmlBody = $@"
<div style=""font-family:Segoe UI,Arial,sans-serif; background:transparent; padding:24px;"">
  <div style=""max-width:600px; margin:0 auto; background:#ffffff; border-radius:10px;
              box-shadow:0 4px 12px rgba(0,0,0,0.08); overflow:hidden; border:1px solid #e2e8f0;"">
    <div style=""background:linear-gradient(135deg,#4f46e5,#3b82f6); padding:18px 24px;"">
      <h2 style=""margin:0; font-size:20px; color:#ffffff;"">Domain Scan Report</h2>
      <p style=""margin:4px 0 0; font-size:13px; letter-spacing:.5px; color:#e0f2fe;"">{DateTime.Now:dd-MM-yyyy HH:mm} Local Time</p>
    </div>
    <div style=""padding:24px 24px 8px;"">
      <h3 style=""margin:0 0 12px; font-size:18px; color:#1e293b;"">Results for:
        <span style=""color:#2563eb; word-break:break-all;"">{urlDisplay}</span>
      </h3>
      <p style=""margin:0 0 16px; font-size:15px; line-height:1.5; color:#334155;"">
        {summary}
      </p>

      <table cellspacing=""0"" cellpadding=""0"" style=""width:100%; border-collapse:collapse; font-size:14px;"">
        <thead>
          <tr>
            <th align=""left"" style=""padding:8px 10px; background:#f1f5f9; color:#475569; font-weight:600; border-bottom:1px solid #e2e8f0;"">Category</th>
            <th align=""right"" style=""padding:8px 10px; background:#f1f5f9; color:#475569; font-weight:600; border-bottom:1px solid #e2e8f0;"">Count</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td style=""padding:8px 10px; border-bottom:1px solid #e2e8f0; color:#dc2626; font-weight:{(malicious > 0 ? "600" : "400")};"">
              Malicious
            </td>
            <td align=""right"" style=""padding:8px 10px; border-bottom:1px solid #e2e8f0; color:#dc2626; font-weight:{(malicious > 0 ? "600" : "400")};"">
              {malicious}
            </td>
          </tr>
          <tr>
            <td style=""padding:8px 10px; border-bottom:1px solid #e2e8f0; color:#f59e0b; font-weight:{(suspicious > 0 ? "600" : "400")};"">
              Suspicious
            </td>
            <td align=""right"" style=""padding:8px 10px; border-bottom:1px solid #e2e8f0; color:#f59e0b; font-weight:{(suspicious > 0 ? "600" : "400")};"">
              {suspicious}
            </td>
          </tr>
          <tr>
            <td style=""padding:8px 10px; border-bottom:1px solid #e2e8f0; color:#0369a1;"">
              Undetected
            </td>
            <td align=""right"" style=""padding:8px 10px; border-bottom:1px solid #e2e8f0; color:#0369a1;"">
              {undetected}
            </td>
          </tr>
          <tr>
            <td style=""padding:8px 10px; border-bottom:1px solid #e2e8f0; color:#16a34a;"">
              Harmless
            </td>
            <td align=""right"" style=""padding:8px 10px; border-bottom:1px solid #e2e8f0; color:#16a34a;"">
              {harmless}
            </td>
          </tr>
          <tr>
            <td style=""padding:8px 10px; color:#64748b;"">
              Timeout
            </td>
            <td align=""right"" style=""padding:8px 10px; color:#64748b;"">
              {timeout}
            </td>
          </tr>
        </tbody>
      </table>

      <div style=""margin:18px 0 8px; font-size:13px; line-height:1.5; color:#64748b;"">
        <p style=""margin:0 0 8px;"">
          Total engines counted: {harmless + undetected + suspicious + malicious + timeout}.
        </p>
        {(malicious > 0 ? @"<p style=""margin:0 0 8px;"">Consider re-checking the source, scanning related files, and if this is a false positive let us know.</p>" : @"")}
        <p style=""margin:0;"">Have questions? Just reply — we actually read these.</p>
      </div>
    </div>
    <div style=""background:#f1f5f9; padding:14px 24px; font-size:12px; color:#64748b;"">
      <p style=""margin:0;"">Stay safe,</p>
      <p style=""margin:4px 0 0; font-weight:600; color:#334155;"">The Wire Tracer Team 🛰️</p>
    </div>
  </div>
</div>";

            await _emailService.SendEmailAsync(
                email,
                "Your Domain Scan Report",
                htmlBody
            );
        }
        /// <summary>
        /// Sends a welcome email to a new user.
        /// </summary>
        /// <param name="email">The email address of the user to send the email to.</param>
        /// <param name="name">The name of the user to send the email to.</param>
        /// <remarks>
        /// The email includes a welcome message, a link to the user's dashboard, and a link to the documentation.
        /// </remarks>
        private async Task SendWelcomeEmail(string email, string name)
        {
            await _emailService.SendEmailAsync(
                email,
                "Welcome to Wire Tracer",
                $@"
                    <!DOCTYPE html>
                    <html lang=""en"">
                    <body style=""margin:0; padding:0; background:transparent; font-family:Segoe UI,Arial,sans-serif;"">
                        <div style=""max-width:560px; margin:32px auto; background:#ffffff; border:1px solid #e2e8f0;
                            border-radius:8px; overflow:hidden;"">
                            <div style=""background:#2563eb; padding:18px 24px;"">
                            <h1 style=""margin:0; font-size:20px; color:#ffffff; font-weight:600;"">Welcome aboard!</h1>
                            </div>
                            <div style=""padding:24px 28px 16px; color:#1e293b; font-size:15px; line-height:1.55;"">
                                <p style=""margin:0 0 14px;"">Hi <strong>{System.Net.WebUtility.HtmlEncode(name)}</strong>,</p>
                                <p style=""margin:0 0 14px;"">
                                Thanks for joining <strong>Wire Tracer</strong>. We’re excited to help you
                                simplify packet analysis and turn raw traffic into clear, actionable insights.
                                </p>

                                <p style=""margin:22px 0 6px;"">Have a great day,</p>
                                <p style=""margin:0; font-weight:600; color:#334155;"">The Wire Tracer Team</p>
                        </div>
                        <div style=""background:#f1f5f9; padding:10px 18px; text-align:center;
                            font-size:11px; color:#64748b;"">
                            <p style=""margin:0;"">&copy; {DateTime.Now:yyyy} Wire Tracer. All rights reserved.</p>
                        </div>
                    </div>
                </body>
            </html>"
            );

        }

    }
}