namespace Trucktaxpro.Services;

public interface IAppEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}
