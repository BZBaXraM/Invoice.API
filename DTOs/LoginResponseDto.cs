namespace InvoiceManager.API.DTOs;

/// <summary>
/// Represents the response to a login request.
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// The access token.
    /// </summary>
    public string AccessToken { get; set; } = default!;
    /// <summary>
    /// The refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = default!;
}