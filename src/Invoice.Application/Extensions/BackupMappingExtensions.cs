namespace Invoice.Application.Extensions;

public static class BackupMappingExtensions
{
    public static CompanyProfileExport ToCompanyProfileExport(this CompanyProfile profile) => new()
    {
        Id = profile.Id,
        CompanyName = profile.CompanyName,
        Voen = profile.Voen,
        BankName = profile.BankName,
        BankVoen = profile.BankVoen,
        Iban = profile.Iban,
        BankAccount = profile.BankAccount,
        SwiftCode = profile.SwiftCode,
        LogoBase64 = profile.LogoImage is null ? null : Convert.ToBase64String(profile.LogoImage),
        LogoContentType = profile.LogoContentType,
        SignatureBase64 = profile.SignatureImage is null ? null : Convert.ToBase64String(profile.SignatureImage),
        SignatureContentType = profile.SignatureContentType,
        CreatedAt = profile.CreatedAt,
        UpdatedAt = profile.UpdatedAt
    };

    public static UserDataExportResponse ToUserDataExportResponse(this UserDataGraph graph) => new()
    {
        ExportedAt = DateTimeOffset.UtcNow,
        Profile = graph.User.ToUserResponse(),
        CompanyProfile = graph.CompanyProfile?.ToCompanyProfileExport(),
        Customers = graph.Customers.Select(c => c.ToCustomerResponse()).ToList(),
        Invoices = graph.Invoices.Select(i => i.ToInvoiceResponse()).ToList(),
        RecurringInvoices = graph.RecurringInvoices.Select(r => r.ToRecurringInvoiceResponse()).ToList(),
        AuditLogs = graph.AuditLogs.Select(a => a.ToAuditLogResponse()).ToList()
    };
}
