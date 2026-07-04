namespace Invoice.Application.Services;

public class CustomerService(
    IUnitOfWork uow,
    CreateCustomerRequestValidator createValidator,
    UpdateCustomerRequestValidator updateValidator) : ICustomerService
{
    public async Task<ResponseModel<CustomerResponse>> CreateAsync(Guid ownerUserId, CreateCustomerRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ResponseModel.Failure<CustomerResponse>("Validation failed", 400,
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var customer = new Customer
        {
            UserId = ownerUserId,
            Name = request.Name,
            Address = request.Address,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        uow.CustomerRepository.AddCustomer(customer);
        await uow.CommitAsync();

        return ResponseModel.Success(customer.ToCustomerResponse());
    }

    public async Task<ResponseModel<CustomerResponse>> UpdateAsync(Guid ownerUserId, Guid id, UpdateCustomerRequest request)
    {
        var validation = await updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ResponseModel.Failure<CustomerResponse>("Validation failed", 400,
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var customer = await uow.CustomerRepository.GetByIdAsync(id, ownerUserId);
        if (customer is null)
        {
            return ResponseModel.Failure<CustomerResponse>("Customer not found", 404);
        }

        customer.Name = request.Name;
        customer.Address = request.Address;
        customer.Email = request.Email;
        customer.PhoneNumber = request.PhoneNumber;
        customer.UpdatedAt = DateTimeOffset.UtcNow;

        await uow.CommitAsync();

        return ResponseModel.Success(customer.ToCustomerResponse());
    }

    public async Task<ResponseModel<CustomerResponse>> GetByIdAsync(Guid ownerUserId, Guid id)
    {
        var customer = await uow.CustomerRepository.GetByIdAsync(id, ownerUserId);
        if (customer is null)
        {
            return ResponseModel.Failure<CustomerResponse>("Customer not found", 404);
        }

        return ResponseModel.Success(customer.ToCustomerResponse());
    }

    public async Task<ResponseModel<PagedResult<CustomerResponse>>> GetListAsync(
        Guid ownerUserId,
        int pageNumber,
        int pageSize,
        string? nameFilter,
        string? sortBy,
        bool sortDescending)
    {
        var (items, totalCount) =
            await uow.CustomerRepository.GetPagedAsync(ownerUserId, pageNumber, pageSize, nameFilter, sortBy,
                sortDescending);

        return ResponseModel.Success(new PagedResult<CustomerResponse>
        {
            Items = items.Select(c => c.ToCustomerResponse()).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    public async Task<ResponseModel> DeleteAsync(Guid ownerUserId, Guid id)
    {
        var customer = await uow.CustomerRepository.GetByIdAsync(id, ownerUserId);
        if (customer is null)
        {
            return ResponseModel.Failure("Customer not found", 404);
        }

        if (await uow.CustomerRepository.HasSentInvoicesAsync(id))
        {
            return ResponseModel.Failure("Cannot delete a customer that has been sent invoices", 409);
        }

        uow.CustomerRepository.RemoveCustomer(customer);
        await uow.CommitAsync();

        return ResponseModel.Success("Customer deleted successfully");
    }

    public async Task<ResponseModel> ArchiveAsync(Guid ownerUserId, Guid id)
    {
        var customer = await uow.CustomerRepository.GetByIdAsync(id, ownerUserId);
        if (customer is null)
        {
            return ResponseModel.Failure("Customer not found", 404);
        }

        customer.DeletedAt = DateTimeOffset.UtcNow;
        await uow.CommitAsync();

        return ResponseModel.Success("Customer archived successfully");
    }
}
