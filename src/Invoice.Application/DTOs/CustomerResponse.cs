namespace Invoice.Application.DTOs;

/// <summary>
/// Public representation of a customer.
/// </summary>
public class CustomerResponse
{
    /// <summary>The customer's unique identifier.</summary>
    public Guid Id { get; set; }

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

    /// <summary>When the customer was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>When the customer was last updated.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>When the customer was archived (soft-deleted), if at all.</summary>
    public DateTimeOffset? DeletedAt { get; set; }
}
