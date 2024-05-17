namespace InvoiceManager.API.Models;

/// <summary>
/// The user entity.
/// </summary>
public class User
{
    /// <summary>
    /// The user ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the user.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// The address of the user.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// The email of the user.
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// The password of the user.
    /// </summary>
    public string Password { get; set; } = default!;

    /// <summary>
    /// The phone number of the user.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// The date and time when the user was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///  The date and time when the user was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The role of the user.
    /// </summary>
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}