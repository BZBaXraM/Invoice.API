namespace Invoice.API.Services;

public interface IAsyncInvoiceService
{
    Task<Models.Invoice> AddInvoiceAsync(Models.Invoice invoice);
    Task<Models.Invoice> UpdateInvoiceAsync(Models.Invoice invoice);
    Task<Models.Invoice> SendInvoiceAsync(Guid id);
    Task<Models.Invoice> SoftDeleteInvoiceAsync(Guid id);
    Task<Models.Invoice> DeleteInvoiceAsync(Guid id);
    Task<List<Models.Invoice>> GetInvoicesAsync(string? filterOn, string? filterQuery,
        string? sortBy, bool isAscending = true, int pageNumber = 1, int pageSize = 100);
    Task<Models.Invoice> GetInvoiceAsync(Guid id);
    Task<byte[]> DownloadInvoiceAsync(Guid id);
}