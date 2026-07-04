namespace Invoice.Application.ServiceContracts;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string message);
    Task SendConfirmationEmailAsync(string email, string code);
    Task SendPasswordResetEmailAsync(string email, string code);
}
