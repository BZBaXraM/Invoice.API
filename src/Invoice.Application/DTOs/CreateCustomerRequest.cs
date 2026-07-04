namespace Invoice.Application.DTOs;

/// <summary>
/// Request to create a new customer.
/// </summary>
public class CreateCustomerRequest
{
    /// <summary>The customer's name.</summary>
    public required string Name { get; set; }

    /// <summary>The customer's address.</summary>
    public string? Address { get; set; }

    /// <summary>The customer's email.</summary>
    public required string Email { get; set; }

    /// <summary>The customer's phone number.</summary>
    public string? PhoneNumber { get; set; }
}
