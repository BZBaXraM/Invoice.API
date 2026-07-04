namespace Invoice.Application.DTOs;

/// <summary>
/// Access/refresh token pair returned after a successful login, refresh, or registration.
/// </summary>
public class LoginResponse
{
    /// <summary>The short-lived JWT access token.</summary>
    public required string AccessToken { get; set; }

    /// <summary>The long-lived refresh token used to obtain a new access token.</summary>
    public required string RefreshToken { get; set; }

    /// <summary>The UTC expiry of the refresh token.</summary>
    public DateTime RefreshTokenExpireTime { get; set; }
}
