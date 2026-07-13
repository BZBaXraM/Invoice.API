namespace Invoice.Application.DTOs;

/// <summary>
/// Public representation of the user's company profile (seller requisites).
/// Image bytes are never included — fetch them via the dedicated image endpoints.
/// </summary>
public class CompanyProfileResponse
{
    /// <summary>The profile's unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>The legal company name.</summary>
    public required string CompanyName { get; set; }

    /// <summary>The company's VÖEN (tax identification number).</summary>
    public required string Voen { get; set; }

    /// <summary>The bank's name.</summary>
    public string? BankName { get; set; }

    /// <summary>The bank's VÖEN.</summary>
    public string? BankVoen { get; set; }

    /// <summary>The company's IBAN.</summary>
    public string? Iban { get; set; }

    /// <summary>The bank account number.</summary>
    public string? BankAccount { get; set; }

    /// <summary>The bank's SWIFT/BIC code.</summary>
    public string? SwiftCode { get; set; }

    /// <summary>Whether a logo image has been uploaded.</summary>
    public bool HasLogo { get; set; }

    /// <summary>Whether a signature image has been uploaded.</summary>
    public bool HasSignature { get; set; }

    /// <summary>When the profile was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>When the profile was last updated.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
