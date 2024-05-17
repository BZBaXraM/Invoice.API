namespace InvoiceManager.API.DTOs;

/// <summary>
/// The user login DTO.
/// </summary>
public class UserLoginDto
{
    /// <summary>
    /// The email.
    /// </summary>
    public string Email { get; private init; } = default!;

    /// <summary>
    /// The password.
    /// </summary>
    public string Password { get; private init; } = default!;
}