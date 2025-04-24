using Mailjet.Client;
using Mailjet.Client.Resources;
using Mailjet.Client.TransactionalEmails;
using Mailjet.Client.TransactionalEmails.Response;
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
        /// <summary>
        /// Sends an email with a single PDF attachment.
        /// </summary>
        /// <param name="to">Recipient email address.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="body">HTML body content.</param>
        /// <param name="pdfBytes">PDF file as a byte array.</param>
        /// <param name="fileName">Filename to present in the attachment.</param>
        public async Task SendEmailWithAttachmentAsync(
            string to,
            string subject,
            string body,
            byte[] pdfBytes,
            string fileName = "document.pdf")
        {
            // Instantiate the Mailjet client
            var client = new MailjetClient(_apiKey, _apiSecret);

            // Build the email
            var email = new TransactionalEmailBuilder()
                .WithFrom(new SendContact("noreply.recoursia@gmail.com", "ReCoursia Team"))
                .WithSubject(subject)
                .WithHtmlPart(body)
                .WithTo(new SendContact(to))
                .WithAttachment(
                    new Attachment(
                        fileName,
                        "application/pdf",
                        Convert.ToBase64String(pdfBytes)
                    )
                )
                .Build();

            // Send it
            TransactionalEmailResponse response =
                await client.SendTransactionalEmailAsync(email);

            // Check result
            var message = response.Messages.FirstOrDefault();
            if (message == null || !string.Equals(message.Status, "success", StringComparison.OrdinalIgnoreCase))
            {
                var err = message?.Errors?.FirstOrDefault();
                throw new Exception($"Mailjet send failed: {err?.ErrorCode} – {err?.ErrorMessage}");
            }
        }

    }

}