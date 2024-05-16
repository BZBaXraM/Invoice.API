namespace InvoiceManager.API.Models;

public enum InvoiceStatus
{
    Created,
    Sent,
    Received,
    Paid,
    Cancelled,
    Rejected
}