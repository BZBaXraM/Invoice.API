namespace Invoice.Application.RepositoryContracts;

public interface IInvoiceRepository
{
    Domain.Entities.Invoice AddInvoice(Domain.Entities.Invoice invoice);

    Task<Domain.Entities.Invoice?> GetByIdWithRowsAsync(Guid id, Guid ownerUserId);

    Task<int> GetNextInvoiceNumberAsync(Guid ownerUserId);

    /// <summary>
    /// Returns unpaid, non-archived invoices (across all users) whose due date has passed.
    /// </summary>
    Task<List<Domain.Entities.Invoice>> GetOverdueCandidatesAsync(DateTimeOffset now);

    Task<(List<Domain.Entities.Invoice> Items, int TotalCount)> GetPagedAsync(
        Guid ownerUserId,
        int pageNumber,
        int pageSize,
        Guid? customerId,
        InvoiceStatus? status,
        string? sortBy,
        bool sortDescending);

    void RemoveInvoice(Domain.Entities.Invoice invoice);
}
