namespace Invoice.Application.Services;

public class RecurringInvoiceService(
    IUnitOfWork uow,
    CreateRecurringInvoiceRequestValidator createValidator,
    UpdateRecurringInvoiceRequestValidator updateValidator,
    IRealtimeNotifier realtimeNotifier) : IRecurringInvoiceService
{
    public async Task<ResponseModel<RecurringInvoiceResponse>> CreateAsync(Guid ownerUserId, CreateRecurringInvoiceRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ResponseModel.Failure<RecurringInvoiceResponse>("validation.failed", 400,
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var customer = await uow.CustomerRepository.GetByIdAsync(request.CustomerId, ownerUserId);
        if (customer is null)
        {
            return ResponseModel.Failure<RecurringInvoiceResponse>("Customer not found", 404);
        }

        var recurring = new RecurringInvoice
        {
            UserId = ownerUserId,
            CustomerId = request.CustomerId,
            Frequency = request.Frequency,
            NextRunDate = request.NextRunDate,
            EndDate = request.EndDate,
            IsActive = true,
            DueInDays = request.DueInDays,
            VatRate = request.VatRate,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            Comment = request.Comment,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Rows = request.Rows.Select(r => new RecurringInvoiceRow
            {
                Service = r.Service,
                Quantity = r.Quantity,
                Rate = r.Rate
            }).ToList()
        };

        uow.RecurringInvoiceRepository.AddRecurringInvoice(recurring);
        await uow.CommitAsync();

        var response = recurring.ToRecurringInvoiceResponse();
        await realtimeNotifier.RecurringInvoiceCreatedAsync(ownerUserId, response);

        return ResponseModel.Success(response);
    }

    public async Task<ResponseModel<RecurringInvoiceResponse>> UpdateAsync(Guid ownerUserId, Guid id, UpdateRecurringInvoiceRequest request)
    {
        var validation = await updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ResponseModel.Failure<RecurringInvoiceResponse>("validation.failed", 400,
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var recurring = await uow.RecurringInvoiceRepository.GetByIdWithRowsAsync(id, ownerUserId);
        if (recurring is null)
        {
            return ResponseModel.Failure<RecurringInvoiceResponse>("Recurring invoice not found", 404);
        }

        var customer = await uow.CustomerRepository.GetByIdAsync(request.CustomerId, ownerUserId);
        if (customer is null)
        {
            return ResponseModel.Failure<RecurringInvoiceResponse>("Customer not found", 404);
        }

        recurring.CustomerId = request.CustomerId;
        recurring.Frequency = request.Frequency;
        recurring.NextRunDate = request.NextRunDate;
        recurring.EndDate = request.EndDate;
        recurring.DueInDays = request.DueInDays;
        recurring.VatRate = request.VatRate;
        recurring.DiscountType = request.DiscountType;
        recurring.DiscountValue = request.DiscountValue;
        recurring.Comment = request.Comment;
        recurring.UpdatedAt = DateTimeOffset.UtcNow;

        recurring.Rows.Clear();
        foreach (var row in request.Rows)
        {
            // AddRow marks the new row as Added explicitly — nav-collection discovery
            // alone would track it as Modified (its Guid key is pre-set) and fail.
            var newRow = new RecurringInvoiceRow
            {
                RecurringInvoiceId = recurring.Id,
                Service = row.Service,
                Quantity = row.Quantity,
                Rate = row.Rate
            };
            recurring.Rows.Add(newRow);
            uow.RecurringInvoiceRepository.AddRow(newRow);
        }

        await uow.CommitAsync();

        var response = recurring.ToRecurringInvoiceResponse();
        await realtimeNotifier.RecurringInvoiceUpdatedAsync(ownerUserId, response);

        return ResponseModel.Success(response);
    }

    public async Task<ResponseModel<RecurringInvoiceResponse>> GetByIdAsync(Guid ownerUserId, Guid id)
    {
        var recurring = await uow.RecurringInvoiceRepository.GetByIdWithRowsAsync(id, ownerUserId);
        if (recurring is null)
        {
            return ResponseModel.Failure<RecurringInvoiceResponse>("Recurring invoice not found", 404);
        }

        return ResponseModel.Success(recurring.ToRecurringInvoiceResponse());
    }

    public async Task<ResponseModel<PagedResult<RecurringInvoiceResponse>>> GetListAsync(Guid ownerUserId, int pageNumber, int pageSize)
    {
        var (items, totalCount) = await uow.RecurringInvoiceRepository.GetPagedAsync(ownerUserId, pageNumber, pageSize);

        return ResponseModel.Success(new PagedResult<RecurringInvoiceResponse>
        {
            Items = items.Select(r => r.ToRecurringInvoiceResponse()).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    public async Task<ResponseModel<RecurringInvoiceResponse>> ToggleActiveAsync(Guid ownerUserId, Guid id)
    {
        var recurring = await uow.RecurringInvoiceRepository.GetByIdWithRowsAsync(id, ownerUserId);
        if (recurring is null)
        {
            return ResponseModel.Failure<RecurringInvoiceResponse>("Recurring invoice not found", 404);
        }

        recurring.IsActive = !recurring.IsActive;
        recurring.UpdatedAt = DateTimeOffset.UtcNow;
        await uow.CommitAsync();

        var response = recurring.ToRecurringInvoiceResponse();
        await realtimeNotifier.RecurringInvoiceUpdatedAsync(ownerUserId, response);

        return ResponseModel.Success(response);
    }

    public async Task<ResponseModel> DeleteAsync(Guid ownerUserId, Guid id)
    {
        var recurring = await uow.RecurringInvoiceRepository.GetByIdWithRowsAsync(id, ownerUserId);
        if (recurring is null)
        {
            return ResponseModel.Failure("Recurring invoice not found", 404);
        }

        uow.RecurringInvoiceRepository.RemoveRecurringInvoice(recurring);
        await uow.CommitAsync();

        await realtimeNotifier.RecurringInvoiceDeletedAsync(ownerUserId, id);

        return ResponseModel.Success("Recurring invoice deleted successfully");
    }
}
