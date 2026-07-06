namespace Invoice.Infrastructure.Realtime;

/// <summary>
/// Client-facing SignalR method/event names. Kept as constants so the hub and the
/// notifier agree on the exact strings without duplicating magic values.
/// </summary>
public static class RealtimeEvents
{
    public const string CustomerCreated = "CustomerCreated";
    public const string CustomerUpdated = "CustomerUpdated";
    public const string CustomerArchived = "CustomerArchived";
    public const string CustomerUnarchived = "CustomerUnarchived";
    public const string CustomerDeleted = "CustomerDeleted";

    public const string InvoiceCreated = "InvoiceCreated";
    public const string InvoiceUpdated = "InvoiceUpdated";
    public const string InvoiceStatusChanged = "InvoiceStatusChanged";
    public const string InvoiceArchived = "InvoiceArchived";
    public const string InvoiceDeleted = "InvoiceDeleted";
}
