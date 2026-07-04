namespace Invoice.Application.ServiceContracts;

public interface IInvoiceService
{
    Task<ResponseModel<InvoiceResponse>> CreateAsync(Guid ownerUserId, CreateInvoiceRequest request);
    Task<ResponseModel<InvoiceResponse>> UpdateAsync(Guid ownerUserId, Guid id, UpdateInvoiceRequest request);
    Task<ResponseModel> UpdateStatusAsync(Guid ownerUserId, Guid id, InvoiceStatus status);
    Task<ResponseModel<InvoiceResponse>> GetByIdAsync(Guid ownerUserId, Guid id);

    Task<ResponseModel<PagedResult<InvoiceResponse>>> GetListAsync(
        Guid ownerUserId,
        int pageNumber,
        int pageSize,
        Guid? customerId,
        InvoiceStatus? status,
        string? sortBy,
        bool sortDescending);

    Task<ResponseModel> DeleteAsync(Guid ownerUserId, Guid id);
    Task<ResponseModel> ArchiveAsync(Guid ownerUserId, Guid id);
    Task<ResponseModel<byte[]>> ExportToPdfAsync(Guid ownerUserId, Guid id);
}
