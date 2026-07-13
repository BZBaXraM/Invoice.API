namespace Invoice.Application.DTOs;

/// <summary>
/// Request to create or update the user's company profile (seller requisites).
/// </summary>
public class UpsertCompanyProfileRequest
{
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
}
