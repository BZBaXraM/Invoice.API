namespace Invoice.Infrastructure.Realtime;

public class SignalRRealtimeNotifier(IHubContext<NotificationsHub> hubContext) : IRealtimeNotifier
{
    public Task CustomerCreatedAsync(Guid ownerUserId, CustomerResponse customer) =>
        Send(ownerUserId, RealtimeEvents.CustomerCreated, customer);

    public Task CustomerUpdatedAsync(Guid ownerUserId, CustomerResponse customer) =>
        Send(ownerUserId, RealtimeEvents.CustomerUpdated, customer);

    public Task CustomerArchivedAsync(Guid ownerUserId, Guid customerId) =>
        Send(ownerUserId, RealtimeEvents.CustomerArchived, new { customerId });

    public Task CustomerUnarchivedAsync(Guid ownerUserId, Guid customerId) =>
        Send(ownerUserId, RealtimeEvents.CustomerUnarchived, new { customerId });

    public Task CustomerDeletedAsync(Guid ownerUserId, Guid customerId) =>
        Send(ownerUserId, RealtimeEvents.CustomerDeleted, new { customerId });

    public Task InvoiceCreatedAsync(Guid ownerUserId, InvoiceResponse invoice) =>
        Send(ownerUserId, RealtimeEvents.InvoiceCreated, invoice);

    public Task InvoiceUpdatedAsync(Guid ownerUserId, InvoiceResponse invoice) =>
        Send(ownerUserId, RealtimeEvents.InvoiceUpdated, invoice);

    public Task InvoiceStatusChangedAsync(Guid ownerUserId, Guid invoiceId, InvoiceStatus status) =>
        Send(ownerUserId, RealtimeEvents.InvoiceStatusChanged, new { invoiceId, status = status.ToString() });

    public Task InvoiceArchivedAsync(Guid ownerUserId, Guid invoiceId) =>
        Send(ownerUserId, RealtimeEvents.InvoiceArchived, new { invoiceId });

    public Task InvoiceDeletedAsync(Guid ownerUserId, Guid invoiceId) =>
        Send(ownerUserId, RealtimeEvents.InvoiceDeleted, new { invoiceId });

    private Task Send(Guid ownerUserId, string method, object payload) =>
        hubContext.Clients.Group(NotificationsHub.GroupName(ownerUserId.ToString())).SendAsync(method, payload);
}
