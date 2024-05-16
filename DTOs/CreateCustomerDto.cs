namespace InvoiceManager.API.DTOs;

/// <summary>
/// Represents a data transfer object for creating a customer.
/// </summary>
public class CreateCustomerDto
{
    /// <summary>
    /// Gets or sets the name of the customer.
    /// </summary>
    public string Name { get; set; } = default!;
    /// <summary>
    /// Gets or sets the address of the customer.
    /// </summary>
    public string? Address { get; set; }
    /// <summary>
    /// Gets or sets the email of the customer.
    /// </summary>
    public string Email { get; set; } = default!;
    /// <summary>
    /// Gets or sets the phone number of the customer.
    /// </summary>
    public string? PhoneNumber { get; set; }
}