namespace Invoice.Application.DTOs;

/// <summary>
/// Request to reset a password using the code sent by email.
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>The email address of the user.</summary>
    public required string Email { get; set; }

    /// <summary>The 6-character password reset code sent to the user's email.</summary>
    public required string Code { get; set; }

    /// <summary>The new password to set.</summary>
    public required string NewPassword { get; set; }
}
