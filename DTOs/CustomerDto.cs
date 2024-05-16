namespace InvoiceManager.API.DTOs;

/// <summary>
/// Represents a customer data transfer object.
/// </summary>
public class CustomerDto
{
    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the customer name.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the customer address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the customer email.
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// Gets or sets the customer phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the customer was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the customer was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}