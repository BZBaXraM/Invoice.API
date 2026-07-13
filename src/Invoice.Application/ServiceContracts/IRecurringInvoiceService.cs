namespace Invoice.Application.ServiceContracts;

public interface IRecurringInvoiceService
{
    Task<ResponseModel<RecurringInvoiceResponse>> CreateAsync(Guid ownerUserId, CreateRecurringInvoiceRequest request);
    Task<ResponseModel<RecurringInvoiceResponse>> UpdateAsync(Guid ownerUserId, Guid id, UpdateRecurringInvoiceRequest request);
    Task<ResponseModel<RecurringInvoiceResponse>> GetByIdAsync(Guid ownerUserId, Guid id);
    Task<ResponseModel<PagedResult<RecurringInvoiceResponse>>> GetListAsync(Guid ownerUserId, int pageNumber, int pageSize);
    Task<ResponseModel<RecurringInvoiceResponse>> ToggleActiveAsync(Guid ownerUserId, Guid id);
    Task<ResponseModel> DeleteAsync(Guid ownerUserId, Guid id);
}
