namespace Invoice.Application.ServiceContracts;

public interface IPaymentService
{
    Task<ResponseModel<InvoiceResponse>> AddAsync(Guid ownerUserId, Guid invoiceId, CreatePaymentRequest request);
    Task<ResponseModel<List<PaymentResponse>>> GetListAsync(Guid ownerUserId, Guid invoiceId);
    Task<ResponseModel<InvoiceResponse>> DeleteAsync(Guid ownerUserId, Guid invoiceId, Guid paymentId);
}
