namespace Invoice.Domain.Entities;

public class CompanyProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public required string CompanyName { get; set; }
    public required string Voen { get; set; }
    public string? BankName { get; set; }
    public string? BankVoen { get; set; }
    public string? Iban { get; set; }
    public string? BankAccount { get; set; }
    public string? SwiftCode { get; set; }
    public byte[]? LogoImage { get; set; }
    public string? LogoContentType { get; set; }
    public byte[]? SignatureImage { get; set; }
    public string? SignatureContentType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
