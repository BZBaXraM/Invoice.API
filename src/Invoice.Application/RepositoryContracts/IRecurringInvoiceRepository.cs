namespace Invoice.Application.RepositoryContracts;

public interface IRecurringInvoiceRepository
{
    RecurringInvoice AddRecurringInvoice(RecurringInvoice recurringInvoice);

    Task<RecurringInvoice?> GetByIdWithRowsAsync(Guid id, Guid ownerUserId);

    Task<(List<RecurringInvoice> Items, int TotalCount)> GetPagedAsync(
        Guid ownerUserId,
        int pageNumber,
        int pageSize);

    /// <summary>
    /// Returns all active templates (across all users) whose next run is due.
    /// </summary>
    Task<List<RecurringInvoice>> GetDueAsync(DateTimeOffset now);

    void RemoveRecurringInvoice(RecurringInvoice recurringInvoice);
}
