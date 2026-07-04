namespace Invoice.Application.DTOs;

/// <summary>
/// Request to change the current user's password.
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>The user's current password.</summary>
    public required string OldPassword { get; set; }

    /// <summary>The new password to set.</summary>
    public required string NewPassword { get; set; }
}
