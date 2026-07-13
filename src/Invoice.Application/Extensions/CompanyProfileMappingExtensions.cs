namespace Invoice.Application.Extensions;

public static class CompanyProfileMappingExtensions
{
    public static CompanyProfileResponse ToCompanyProfileResponse(this CompanyProfile profile) => new()
    {
        Id = profile.Id,
        CompanyName = profile.CompanyName,
        Voen = profile.Voen,
        BankName = profile.BankName,
        BankVoen = profile.BankVoen,
        Iban = profile.Iban,
        BankAccount = profile.BankAccount,
        SwiftCode = profile.SwiftCode,
        HasLogo = profile.LogoImage is { Length: > 0 },
        HasSignature = profile.SignatureImage is { Length: > 0 },
        CreatedAt = profile.CreatedAt,
        UpdatedAt = profile.UpdatedAt
    };
}
