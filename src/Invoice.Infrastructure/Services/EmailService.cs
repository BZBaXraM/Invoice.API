using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

namespace Invoice.Infrastructure.Services;

public class EmailService(EmailConfig config, ILogger<EmailService> logger, SmtpClient client) : IEmailService
{
    private const string Subject = "Invoice.API - Email Confirmation";

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        try
        {
            MimeMessage emailMessage = new();

            emailMessage.From.Add(new MailboxAddress("Invoice.API", config.From));
            emailMessage.To.Add(new MailboxAddress("User", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(TextFormat.Html)
            {
                Text = message
            };

            client.ServerCertificateValidationCallback = (_, _, _, _) => true;

            await client.ConnectAsync(config.SmtpServer, config.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(config.UserName, config.Password);
            await client.SendAsync(emailMessage);

            await client.DisconnectAsync(true);

            logger.LogInformation("Email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Email}", email);
            throw;
        }
    }

    public async Task SendConfirmationEmailAsync(string email, string code)
    {
        var message = $"""

                        <html>
                        <body>
                            <h2>Welcome!</h2>
                            <p>Thank you for registering. Please use the following confirmation code to verify your email address:</p>
                            <h3 style='color: #007bff; font-size: 24px; letter-spacing: 2px;'>{code}</h3>
                            <p>This code will expire in 5 minutes.</p>
                            <p>If you didn't create an account with Invoice.API, please ignore this email.</p>
                        </body>
                        </html>
                        """;

        await SendEmailAsync(email, Subject, message);
    }

    public async Task SendPasswordResetEmailAsync(string email, string code)
    {
        var message = $"""

                        <html>
                        <body>
                            <h2>Password Reset Request</h2>
                            <p>You have requested to reset your password. Please use the following code:</p>
                            <h3 style='color: #007bff; font-size: 24px; letter-spacing: 2px;'>{code}</h3>
                            <p>This code will expire in 15 minutes.</p>
                            <p>If you didn't request a password reset, please ignore this email and your password will remain unchanged.</p>
                        </body>
                        </html>
                        """;

        await SendEmailAsync(email, "Invoice.API - Password Reset", message);
    }
}
