namespace Invoice.Application.DTOs;

/// <summary>
/// Request to exchange a refresh token for a new token pair.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>The refresh token previously issued to the client.</summary>
    public required string RefreshToken { get; set; }

    /// <summary>Whether the original login was a "remember me" session; carries the same expiry policy forward across refreshes.</summary>
    public bool RememberMe { get; set; }
}
