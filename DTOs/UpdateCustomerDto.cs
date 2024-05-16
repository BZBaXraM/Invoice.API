namespace InvoiceManager.API.DTOs;

/// <summary>
/// Represents the data transfer object for updating a customer.
/// </summary>
public class UpdateCustomerDto
{
    /// <summary>
    /// Gets or sets the customer's name.
    /// </summary>
    public string Name { get; set; } = default!;
    /// <summary>
    /// Gets or sets the customer's address.
    /// </summary>
    public string? Address { get; set; }
    /// <summary>
    /// Gets or sets the customer's email.
    /// </summary>
    public string Email { get; set; } = default!;
    /// <summary>
    /// Gets or sets the customer's phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }
}