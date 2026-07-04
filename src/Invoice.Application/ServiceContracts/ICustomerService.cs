namespace Invoice.Application.ServiceContracts;

public interface ICustomerService
{
    Task<ResponseModel<CustomerResponse>> CreateAsync(Guid ownerUserId, CreateCustomerRequest request);
    Task<ResponseModel<CustomerResponse>> UpdateAsync(Guid ownerUserId, Guid id, UpdateCustomerRequest request);
    Task<ResponseModel<CustomerResponse>> GetByIdAsync(Guid ownerUserId, Guid id);

    Task<ResponseModel<PagedResult<CustomerResponse>>> GetListAsync(
        Guid ownerUserId,
        int pageNumber,
        int pageSize,
        string? nameFilter,
        string? sortBy,
        bool sortDescending);

    Task<ResponseModel> DeleteAsync(Guid ownerUserId, Guid id);
    Task<ResponseModel> ArchiveAsync(Guid ownerUserId, Guid id);
}
