namespace Invoice.Application.DTOs;

/// <summary>
/// Request to change an invoice's status.
/// </summary>
public class UpdateInvoiceStatusRequest
{
    /// <summary>The new status.</summary>
    public InvoiceStatus Status { get; set; }
}
