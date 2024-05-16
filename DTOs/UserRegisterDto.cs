using InvoiceManager.API.Models;

namespace InvoiceManager.API.DTOs;

/// <summary>
/// The user register DTO.
/// </summary>
public class UserRegisterDto
{
    /// <summary>
    /// The name.
    /// </summary>
    public string Name { get; set; } = default!;
    /// <summary>
    /// The email.
    /// </summary>
    public string Email { get; set; } = default!;
    /// <summary>
    /// The password.
    /// </summary>
    public string Password { get; set; } = default!;
    /// <summary>
    /// The created at.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    /// <summary>
    /// The updated at.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;


    private static UserRegisterDto ConvertToUser(User user) => new()
    {
        Name = user.Name,
        Email = user.Email,
        Password = user.Password,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}