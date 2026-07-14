namespace Invoice.Application.DTOs;

/// <summary>
/// The user's company profile as exported into a backup, images included as base64.
/// </summary>
public class CompanyProfileExport
{
    public Guid Id { get; set; }
    public required string CompanyName { get; set; }
    public required string Voen { get; set; }
    public string? BankName { get; set; }
    public string? BankVoen { get; set; }
    public string? Iban { get; set; }
    public string? BankAccount { get; set; }
    public string? SwiftCode { get; set; }
    public string? LogoBase64 { get; set; }
    public string? LogoContentType { get; set; }
    public string? SignatureBase64 { get; set; }
    public string? SignatureContentType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// Everything belonging to one user: profile data, customers, invoices (rows + payments),
/// recurring templates and change history.
/// </summary>
public class UserDataExportResponse
{
    public DateTimeOffset ExportedAt { get; set; }
    public required UserResponse Profile { get; set; }
    public CompanyProfileExport? CompanyProfile { get; set; }
    public List<CustomerResponse> Customers { get; set; } = [];
    public List<InvoiceResponse> Invoices { get; set; } = [];
    public List<RecurringInvoiceResponse> RecurringInvoices { get; set; } = [];
    public List<AuditLogResponse> AuditLogs { get; set; } = [];
}

/// <summary>
/// A full-database backup produced by pg_dump (custom format, restorable with pg_restore).
/// </summary>
public class DatabaseBackupResult
{
    /// <summary>The dump bytes (pg_dump custom format).</summary>
    public required byte[] Content { get; set; }

    /// <summary>The suggested download file name, e.g. backup-20260714-0930.dump.</summary>
    public required string FileName { get; set; }
}
