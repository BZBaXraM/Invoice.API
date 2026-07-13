namespace Invoice.Domain.Enums;

public enum InvoiceStatus
{
    Created = 0,
    Sent = 1,
    Received = 2,
    Paid = 3,
    Cancelled = 4,
    Rejected = 5,
    PartiallyPaid = 6,
    Overdue = 7
}
