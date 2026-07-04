namespace Invoice.Application.DTOs;

/// <summary>
/// Request to resend an email confirmation code.
/// </summary>
public class RequestConfirmationCodeRequest
{
    /// <summary>The email address to send the confirmation code to.</summary>
    public required string Email { get; set; }
}
