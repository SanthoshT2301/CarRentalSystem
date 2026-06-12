namespace CarRentalSystem.Service.Email;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}