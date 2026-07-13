namespace Invoice.Application.ServiceContracts;

/// <summary>
/// Pushes live updates to connected clients belonging to a given user. Implemented over
/// SignalR in the infrastructure layer; the application layer only depends on this contract.
/// </summary>
public interface IRealtimeNotifier
{
    Task CustomerCreatedAsync(Guid ownerUserId, CustomerResponse customer);
    Task CustomerUpdatedAsync(Guid ownerUserId, CustomerResponse customer);
    Task CustomerArchivedAsync(Guid ownerUserId, Guid customerId);
    Task CustomerUnarchivedAsync(Guid ownerUserId, Guid customerId);
    Task CustomerDeletedAsync(Guid ownerUserId, Guid customerId);

    Task InvoiceCreatedAsync(Guid ownerUserId, InvoiceResponse invoice);
    Task InvoiceUpdatedAsync(Guid ownerUserId, InvoiceResponse invoice);
    Task InvoiceStatusChangedAsync(Guid ownerUserId, Guid invoiceId, InvoiceStatus status);
    Task InvoiceArchivedAsync(Guid ownerUserId, Guid invoiceId);
    Task InvoiceDeletedAsync(Guid ownerUserId, Guid invoiceId);

    Task RecurringInvoiceCreatedAsync(Guid ownerUserId, RecurringInvoiceResponse recurringInvoice);
    Task RecurringInvoiceUpdatedAsync(Guid ownerUserId, RecurringInvoiceResponse recurringInvoice);
    Task RecurringInvoiceDeletedAsync(Guid ownerUserId, Guid recurringInvoiceId);
}
