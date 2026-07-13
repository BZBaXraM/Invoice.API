namespace Invoice.Application.Services;

public class PaymentService(
    IUnitOfWork uow,
    CreatePaymentRequestValidator createValidator,
    IRealtimeNotifier realtimeNotifier) : IPaymentService
{
    private static readonly InvoiceStatus[] PayableStatuses =
    [
        InvoiceStatus.Sent,
        InvoiceStatus.Received,
        InvoiceStatus.PartiallyPaid,
        InvoiceStatus.Overdue
    ];

    public async Task<ResponseModel<InvoiceResponse>> AddAsync(Guid ownerUserId, Guid invoiceId, CreatePaymentRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ResponseModel.Failure<InvoiceResponse>("validation.failed", 400,
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var invoice = await uow.InvoiceRepository.GetByIdWithRowsAsync(invoiceId, ownerUserId);
        if (invoice is null)
        {
            return ResponseModel.Failure<InvoiceResponse>("Invoice not found", 404);
        }

        if (!PayableStatuses.Contains(invoice.Status))
        {
            return ResponseModel.Failure<InvoiceResponse>("invoices.payments.error.invalidStatus", 409);
        }

        var amount = InvoiceTotalsCalculator.Round2(request.Amount);
        var balanceDue = invoice.TotalSum - invoice.Payments.Sum(p => p.Amount);
        if (amount > balanceDue)
        {
            return ResponseModel.Failure<InvoiceResponse>("invoices.payments.error.overpay", 409);
        }

        // AddPayment marks the entity as Added explicitly — nav-collection discovery
        // alone would track it as Modified (its Guid key is pre-set) and fail.
        var payment = new Payment
        {
            InvoiceId = invoice.Id,
            UserId = ownerUserId,
            Amount = amount,
            PaymentDate = request.PaymentDate,
            Note = request.Note,
            CreatedAt = DateTimeOffset.UtcNow
        };
        invoice.Payments.Add(payment);
        uow.PaymentRepository.AddPayment(payment);

        var statusChanged = ApplyPaymentDerivedStatus(invoice);
        invoice.UpdatedAt = DateTimeOffset.UtcNow;
        await uow.CommitAsync();

        var response = invoice.ToInvoiceResponse();
        await realtimeNotifier.InvoiceUpdatedAsync(ownerUserId, response);
        if (statusChanged)
        {
            await realtimeNotifier.InvoiceStatusChangedAsync(ownerUserId, invoice.Id, invoice.Status);
        }

        return ResponseModel.Success(response);
    }

    public async Task<ResponseModel<List<PaymentResponse>>> GetListAsync(Guid ownerUserId, Guid invoiceId)
    {
        var invoice = await uow.InvoiceRepository.GetByIdWithRowsAsync(invoiceId, ownerUserId);
        if (invoice is null)
        {
            return ResponseModel.Failure<List<PaymentResponse>>("Invoice not found", 404);
        }

        return ResponseModel.Success(invoice.Payments
            .OrderBy(p => p.PaymentDate)
            .Select(p => p.ToPaymentResponse())
            .ToList());
    }

    public async Task<ResponseModel<InvoiceResponse>> DeleteAsync(Guid ownerUserId, Guid invoiceId, Guid paymentId)
    {
        var invoice = await uow.InvoiceRepository.GetByIdWithRowsAsync(invoiceId, ownerUserId);
        if (invoice is null)
        {
            return ResponseModel.Failure<InvoiceResponse>("Invoice not found", 404);
        }

        var payment = invoice.Payments.FirstOrDefault(p => p.Id == paymentId);
        if (payment is null)
        {
            return ResponseModel.Failure<InvoiceResponse>("Payment not found", 404);
        }

        invoice.Payments.Remove(payment);
        uow.PaymentRepository.RemovePayment(payment);

        var statusChanged = ApplyPaymentDerivedStatus(invoice);
        invoice.UpdatedAt = DateTimeOffset.UtcNow;
        await uow.CommitAsync();

        var response = invoice.ToInvoiceResponse();
        await realtimeNotifier.InvoiceUpdatedAsync(ownerUserId, response);
        if (statusChanged)
        {
            await realtimeNotifier.InvoiceStatusChangedAsync(ownerUserId, invoice.Id, invoice.Status);
        }

        return ResponseModel.Success(response);
    }

    /// <summary>
    /// Re-derives the payment-driven status from the current paid amount: fully paid → Paid,
    /// partially paid → PartiallyPaid, nothing paid → back to Sent (only when the current
    /// status is itself payment-derived). The scheduler re-flags past-due invoices as Overdue
    /// on its next tick.
    /// </summary>
    private static bool ApplyPaymentDerivedStatus(Domain.Entities.Invoice invoice)
    {
        var paid = invoice.Payments.Sum(p => p.Amount);
        var newStatus = paid switch
        {
            _ when paid >= invoice.TotalSum && invoice.TotalSum > 0 => InvoiceStatus.Paid,
            > 0 => InvoiceStatus.PartiallyPaid,
            _ when invoice.Status is InvoiceStatus.Paid or InvoiceStatus.PartiallyPaid => InvoiceStatus.Sent,
            _ => invoice.Status
        };

        if (newStatus == invoice.Status)
        {
            return false;
        }

        invoice.Status = newStatus;
        return true;
    }
}
