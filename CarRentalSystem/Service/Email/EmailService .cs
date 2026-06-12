using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CarRentalSystem.Service.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var settings = _config.GetSection("EmailSettings");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                settings.GetValue<string>("SenderName"),
                settings.GetValue<string>("SenderEmail")));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                settings.GetValue<string>("SmtpServer"),
                settings.GetValue<int>("SmtpPort"),
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                settings.GetValue<string>("SenderEmail"),
                settings.GetValue<string>("SenderPassword"));

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            // Do not throw — email failure shouldn't break registration/reset flow
        }
    }
}