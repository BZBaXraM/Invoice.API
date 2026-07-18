namespace Invoice.Application.DTOs;

/// <summary>
/// Request to log in with either an email or a username, plus a password.
/// </summary>
public class LoginRequest
{
    /// <summary>The user's email address or username.</summary>
    public required string UsernameOrEmail { get; set; }

    /// <summary>The user's password.</summary>
    public required string Password { get; set; }

    /// <summary>When true, the issued refresh token gets a longer expiry (persistent session); when false, a short-lived one.</summary>
    public bool RememberMe { get; set; }
}
