namespace InvoiceManager.API.DTOs;

/// <summary>
/// The refresh token request.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// The refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = default!;
}