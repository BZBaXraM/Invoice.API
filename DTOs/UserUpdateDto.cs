namespace InvoiceManager.API.DTOs;

/// <summary>
/// Data transfer object for updating a user.
/// </summary>
public class UserUpdateDto
{
    /// <summary>
    /// Name of the user.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Email of the user.
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// Address of the user.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Phone number of the user.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Date and time when the user was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}