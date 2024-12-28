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

        public EmailService(IConfiguration configuration)
        {
            _apiKey = configuration["Mailjet:ApiKey"];
            _apiSecret = configuration["Mailjet:ApiSecret"];
        }

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