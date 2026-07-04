namespace Invoice.Application.DTOs;

/// <summary>
/// Request to start a password reset by email.
/// </summary>
public class ForgetPasswordRequest
{
    /// <summary>The email address of the user requesting a password reset.</summary>
    public required string Email { get; set; }
}
