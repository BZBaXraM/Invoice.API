namespace Invoice.Application.DTOs;

/// <summary>
/// Request to update an existing customer.
/// </summary>
public class UpdateCustomerRequest
{
    /// <summary>The customer's first name.</summary>
    public required string FirstName { get; set; }

    /// <summary>The customer's last name.</summary>
    public required string LastName { get; set; }

    /// <summary>The customer's company name.</summary>
    public string? CompanyName { get; set; }

    /// <summary>The customer's address.</summary>
    public string? Address { get; set; }

    /// <summary>The customer's email.</summary>
    public required string Email { get; set; }

    /// <summary>The customer's phone number.</summary>
    public string? PhoneNumber { get; set; }
}
