namespace Invoice.Application.RepositoryContracts;

/// <summary>
/// One user's full data graph, loaded read-only for export.
/// </summary>
public class UserDataGraph
{
    public required User User { get; set; }
    public CompanyProfile? CompanyProfile { get; set; }
    public List<Customer> Customers { get; set; } = [];
    public List<Domain.Entities.Invoice> Invoices { get; set; } = [];
    public List<RecurringInvoice> RecurringInvoices { get; set; } = [];
    public List<AuditLog> AuditLogs { get; set; } = [];
}

public interface IBackupRepository
{
    Task<UserDataGraph?> GetUserGraphAsync(Guid userId);
}
