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
/// One user's account record inside the admin full backup. Unlike the per-user export this
/// includes the credential material (password hash, flags) — it is a restore artifact and
/// is only ever served to the admin account.
/// </summary>
public class BackupUserRecord
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public required string Role { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public CompanyProfileExport? CompanyProfile { get; set; }
    public List<CustomerResponse> Customers { get; set; } = [];
    public List<InvoiceResponse> Invoices { get; set; } = [];
    public List<RecurringInvoiceResponse> RecurringInvoices { get; set; } = [];
    public List<AuditLogResponse> AuditLogs { get; set; } = [];
}

/// <summary>
/// A full-database JSON backup (all users and their data).
/// </summary>
public class FullBackupResponse
{
    public DateTimeOffset ExportedAt { get; set; }
    public List<BackupUserRecord> Users { get; set; } = [];
}
