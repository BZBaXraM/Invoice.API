namespace Invoice.Application.RepositoryContracts;

public interface IInvoiceRepository
{
    Domain.Entities.Invoice AddInvoice(Domain.Entities.Invoice invoice);

    Task<Domain.Entities.Invoice?> GetByIdWithRowsAsync(Guid id, Guid ownerUserId);

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
