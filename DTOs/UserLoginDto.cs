using InvoiceManager.API.Models;

namespace InvoiceManager.API.DTOs;

/// <summary>
/// The user login DTO.
/// </summary>
public class UserLoginDto
{
    /// <summary>
    /// The email.
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// The password.
    /// </summary>
    public string Password { get; set; } = default!;

    private UserLoginDto ConvertToUserLoginDto(User user)
    {
        return new UserLoginDto
        {
            Email = user.Email,
            Password = user.Password
        };
    }
}