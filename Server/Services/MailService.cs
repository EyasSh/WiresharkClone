using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
namespace Server.Services
{
    public class EmailService
    {
        public string? _apiKey = "";
        public string? _apiSecret = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <remarks>
        /// Reads the Mailjet API key and secret from the configuration and sets
        /// the instance fields.
        /// </remarks>
        public EmailService(IConfiguration configuration)
        {
            _apiKey = configuration["Mailjet:ApiKey"];
            _apiSecret = configuration["Mailjet:ApiSecret"];
        }

/// <summary>
/// Sends an email asynchronously using the Mailjet client.
/// </summary>
/// <param name="to">The recipient's email address.</param>
/// <param name="subject">The subject of the email.</param>
/// <param name="body">The HTML content of the email body.</param>
/// <exception cref="Exception">Thrown when the email fails to send.</exception>
/// <remarks>
/// This method constructs a MailjetRequest with the provided email details
/// and sends it using the MailjetClient. If the response indicates failure,
/// an exception is thrown with the error message from Mailjet.
/// </remarks>

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            MailjetClient client = new MailjetClient(_apiKey, _apiSecret);

            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource
            }
            .Property(Send.FromEmail, "noreply.recoursia@gmail.com")
            .Property(Send.FromName, "ReCoursia Team")
            .Property(Send.Subject, subject)
            .Property(Send.HtmlPart, body)
            .Property(Send.Recipients, new JArray
                {
                new JObject
                {
                    {"Email", to}
                }
                });

            MailjetResponse response = await client.PostAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to send email: {response.GetErrorMessage()}");
            }
        }
    }

}