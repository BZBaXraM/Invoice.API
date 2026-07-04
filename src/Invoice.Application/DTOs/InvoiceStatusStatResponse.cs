namespace Invoice.Application.DTOs;

/// <summary>
/// The number of invoices in a given status over a date range.
/// </summary>
public class InvoiceStatusStatResponse
{
    /// <summary>The invoice status.</summary>
    public InvoiceStatus Status { get; set; }

    /// <summary>The number of invoices with this status in the range.</summary>
    public int Count { get; set; }
}
